﻿using SharpLearning.Common.Interfaces;
using SharpLearning.Containers;
using SharpLearning.Containers.Extensions;
using SharpLearning.Containers.Matrices;
using System;
using System.Linq;

namespace SharpLearning.GradientBoost.GBM
{
    /// <summary>
    /// Classificaion gradient boost learner based on 
    /// http://statweb.stanford.edu/~jhf/ftp/trebst.pdf
    /// A series of regression trees are fitted stage wise on the residuals of the previous stage.
    /// The resulting models are ensembled together using addition. Implementation based on:
    /// http://gradientboostedmodels.googlecode.com/files/report.pdf
    /// </summary>
    /// </summary>
    public class GBMGradientBoostClassificationLearner : IIndexedLearner<double>, IIndexedLearner<ProbabilityPrediction>,
        ILearner<double>, ILearner<ProbabilityPrediction>
    {
        readonly GBMDecisionTreeLearner m_learner;
        readonly double m_learningRate;
        readonly int m_iterations;
        readonly double m_subSampleRatio;
        readonly Random m_random = new Random(42);
        readonly IGBMLoss m_loss;

        /// <summary>
        ///  Binomial deviance classification gradient boost learner. 
        ///  A series of regression trees are fitted stage wise on the residuals of the previous stage
        /// </summary>
        /// <param name="iterations">The number of iterations or stages</param>
        /// <param name="learningRate">How much each iteration should contribute with</param>
        /// <param name="maximumTreeDepth">The maximum depth of the tree models</param>
        /// <param name="maximumLeafCount">The maximum leaf count of the tree models</param>
        /// <param name="minimumSplitSize">minimum node split size in the trees 1 is default</param>
        /// <param name="minimumInformationGain">The minimum improvement in information gain before a split is made</param>
        /// <param name="subSampleRatio">ratio of observations sampled at each iteration. Default is 1.0. 
        /// If below 1.0 the algorithm changes to stochastic gradient boosting. 
        /// This reduces variance in the ensemble and can help ounter overfitting</param>
        /// <param name="loss">loss function used</param>
        /// <param name="numberOfThreads">Number of threads to use for paralization</param>
        public GBMGradientBoostClassificationLearner(int iterations, double learningRate, int maximumTreeDepth,
            int minimumSplitSize, double minimumInformationGain, double subSampleRatio, IGBMLoss loss, int numberOfThreads)
        {
            if (iterations < 1) { throw new ArgumentException("Iterations must be at least 1"); }
            if (minimumSplitSize <= 0) { throw new ArgumentException("minimum split size must be larger than 0"); }
            if (maximumTreeDepth < 0) { throw new ArgumentException("maximum tree depth must be larger than 0"); }
            if (minimumInformationGain <= 0) { throw new ArgumentException("minimum information gain must be larger than 0"); }
            if (subSampleRatio <= 0.0 || subSampleRatio > 1.0) { throw new ArgumentException("subSampleRatio must be larger than 0.0 and at max 1.0"); }
            if (loss == null) { throw new ArgumentException("loss"); }

            m_iterations = iterations;
            m_learningRate = learningRate;
            m_subSampleRatio = subSampleRatio;
            m_loss = loss;
            m_learner = new GBMDecisionTreeLearner(maximumTreeDepth, minimumSplitSize, minimumInformationGain, m_loss, numberOfThreads);
        }

        /// <summary>
        ///  Binomial deviance classification gradient boost learner. 
        ///  A series of regression trees are fitted stage wise on the residuals of the previous stage
        /// </summary>
        /// <param name="iterations">The number of iterations or stages</param>
        /// <param name="learningRate">How much each iteration should contribute with</param>
        /// <param name="maximumTreeDepth">The maximum depth of the tree models</param>
        /// <param name="maximumLeafCount">The maximum leaf count of the tree models</param>
        /// <param name="minimumSplitSize">minimum node split size in the trees 1 is default</param>
        /// <param name="minimumInformationGain">The minimum improvement in information gain before a split is made</param>
        /// <param name="subSampleRatio">ratio of observations sampled at each iteration. Default is 1.0. 
        /// If below 1.0 the algorithm changes to stochastic gradient boosting. 
        /// This reduces variance in the ensemble and can help ounter overfitting</param>
        public GBMGradientBoostClassificationLearner(int iterations = 100, double learningRate = 0.1, int maximumTreeDepth = 3,
            int minimumSplitSize = 1, double minimumInformationGain = 0.000001, double subSampleRatio = 1.0)
            : this(iterations, learningRate, maximumTreeDepth, minimumSplitSize, minimumInformationGain, 
                subSampleRatio, new GBMBinomialLoss(), Environment.ProcessorCount)
        {
        }

        /// <summary>
        ///  Binomial deviance classification gradient boost learner. 
        ///  A series of regression trees are fitted stage wise on the residuals of the previous stage
        /// </summary>
        /// <param name="observations"></param>
        /// <param name="targets"></param>
        /// <returns></returns>
        public GBMGradientBoostClassificationModel Learn(F64Matrix observations, double[] targets)
        {
            var allIndices = Enumerable.Range(0, targets.Length).ToArray();
            return Learn(observations, targets, allIndices);
        }

        /// <summary>
        ///  Binomial deviance classification gradient boost learner. 
        ///  A series of regression trees are fitted stage wise on the residuals of the previous stage
        /// </summary>
        /// <param name="observations"></param>
        /// <param name="targets"></param>
        /// <returns></returns>
        public GBMGradientBoostClassificationModel Learn(F64Matrix observations, double[] targets, int[] indices)
        {
            var rows = observations.GetNumberOfRows();
            var orderedElements = CreateOrderedElements(observations, rows);

            var inSample = targets.Select(t => false).ToArray();
            indices.ForEach(i => inSample[i] = true);
            var workIndices = indices.ToArray();

            var uniqueTargets = targets.Distinct().OrderBy(v => v).ToArray();
            var initialLoss = m_loss.InitialLoss(targets, inSample);

            double[][] oneVsAllTargets = null;
            double[][] predictions = null;
            double[][] residuals = null;
            GBMTree[][] trees = null;

            if(uniqueTargets.Length == 2) // Binary case - only need to fit to one class and use (1.0 - probability)
            {
                trees = new GBMTree[][] { new GBMTree[m_iterations] };
                predictions = new double[][] { targets.Select(_ => initialLoss).ToArray() };
                residuals = new double[][] { new double[targets.Length] };

                oneVsAllTargets = new double[1][];
                var target = uniqueTargets[0];
                oneVsAllTargets[0] = targets.Select(t => t == target ? 1.0 : 0.0).ToArray();

            }
            else // multiclass case - use oneVsAll strategy and fit probability for each class
            {
                trees = new GBMTree[uniqueTargets.Length][];
                predictions = uniqueTargets.Select(_ => targets.Select(t => initialLoss).ToArray())
                    .ToArray();
                residuals = uniqueTargets.Select(_ => new double[targets.Length])
                    .ToArray();

                oneVsAllTargets = new double[uniqueTargets.Length][];
                for (int i = 0; i < uniqueTargets.Length; i++)
                {
                    var target = uniqueTargets[i];
                    oneVsAllTargets[i] = targets.Select(t => t == target ? 1.0 : 0.0).ToArray();
                    trees[i] = new GBMTree[m_iterations];
                }
            }
            
            for (int iteration = 0; iteration < m_iterations; iteration++)
            {
                for (int itarget = 0; itarget < trees.Length; itarget++)
                {
                    m_loss.UpdateResiduals(oneVsAllTargets[itarget], predictions[itarget], residuals[itarget], inSample);

                    var sampleSize = targets.Length;
                    if (m_subSampleRatio != 1.0)
                    {
                        sampleSize = (int)Math.Round(m_subSampleRatio * workIndices.Length);
                        inSample = Sample(sampleSize, workIndices, targets.Length);
                    }

                    var tree = m_learner.Learn(observations, oneVsAllTargets[itarget], residuals[itarget], 
                        predictions[itarget], orderedElements, inSample);
                    
                    trees[itarget][iteration] = tree;

                    var predict = tree.Predict(observations);
                    for (int i = 0; i < predict.Length; i++)
                    {
                        predictions[itarget][i] += m_learningRate * predict[i];
                    }
                }
            }

            return new GBMGradientBoostClassificationModel(trees, uniqueTargets, m_learningRate, initialLoss, observations.GetNumberOfColumns());
        }

        /// <summary>
        /// Private explicit interface implementation for indexed learning.
        /// </summary>
        /// <param name="observations"></param>
        /// <param name="targets"></param>
        /// <param name="indices"></param>
        /// <returns></returns>
        IPredictor<double> IIndexedLearner<double>.Learn(F64Matrix observations, double[] targets, int[] indices)
        {
            return Learn(observations, targets, indices);
        }

        /// <summary>
        /// Private explicit interface implementation for indexed probability learning.
        /// </summary>
        /// <param name="observations"></param>
        /// <param name="targets"></param>
        /// <param name="indices"></param>
        /// <returns></returns>
        IPredictor<ProbabilityPrediction> IIndexedLearner<ProbabilityPrediction>.Learn(F64Matrix observations, double[] targets, int[] indices)
        {
            return Learn(observations, targets, indices);
        }

        /// <summary>
        /// Private explicit interface implementation for indexed learning.
        /// </summary>
        /// <param name="observations"></param>
        /// <param name="targets"></param>
        /// <returns></returns>
        IPredictor<double> ILearner<double>.Learn(F64Matrix observations, double[] targets)
        {
            return Learn(observations, targets);
        }

        /// <summary>
        /// Private explicit interface implementation for indexed probability learning.
        /// </summary>
        /// <param name="observations"></param>
        /// <param name="targets"></param>
        /// <returns></returns>
        IPredictor<ProbabilityPrediction> ILearner<ProbabilityPrediction>.Learn(F64Matrix observations, double[] targets)
        {
            return Learn(observations, targets);
        }

        /// <summary>
        /// Creates a matrix of ordered indices. Each row is ordered after the corresponding feature column.
        /// </summary>
        /// <param name="observations"></param>
        /// <param name="rows"></param>
        /// <returns></returns>
        int[][] CreateOrderedElements(F64Matrix observations, int rows)
        {
            var orderedElements = new int[observations.GetNumberOfColumns()][];

            for (int i = 0; i < observations.GetNumberOfColumns(); i++)
            {
                var feature = observations.GetColumn(i);
                var indices = Enumerable.Range(0, rows).ToArray();
                feature.SortWith(indices);
                orderedElements[i] = indices;
            }
            return orderedElements;
        }

        /// <summary>
        /// Creates a bool array with the selected samples (true)
        /// </summary>
        /// <param name="sampleSize"></param>
        /// <param name="indices"></param>
        /// <returns></returns>
        bool[] Sample(int sampleSize, int[] indices, int allObservationCount)
        {
            var inSample = new bool[allObservationCount];
            indices.Shuffle(m_random);

            for (int i = 0; i < sampleSize; i++)
            {
                inSample[indices[i]] = true;
            }

            return inSample;
        }
    }
}
