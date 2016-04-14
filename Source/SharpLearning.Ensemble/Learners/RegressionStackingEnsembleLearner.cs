﻿using SharpLearning.Common.Interfaces;
using SharpLearning.Containers.Extensions;
using SharpLearning.Containers.Matrices;
using SharpLearning.CrossValidation.CrossValidators;
using SharpLearning.Ensemble.Models;
using System;
using System.Linq;

namespace SharpLearning.Ensemble.Learners
{
    /// <summary>
    /// Stacking Regression Ensemble Learner.
    /// http://mlwave.com/kaggle-ensembling-guide/
    /// </summary>
    public sealed class RegressionStackingEnsembleLearner : ILearner<double>, IIndexedLearner<double>
    {
        readonly IIndexedLearner<double>[] m_learners;
        readonly ICrossValidation<double> m_crossValidation;
        readonly Func<F64Matrix, double[], IPredictorModel<double>> m_metaLearner;
        readonly bool m_includeOriginalFeaturesForMetaLearner;

        /// <summary>
        /// Stacking Regression Ensemble Learner. 
        /// Combines several models into a single ensemble model using a top or meta level model to combine the models.
        /// </summary>
        /// <param name="learners">Learners in the ensemble</param>
        /// <param name="crossValidation">Cross validation method</param>
        /// <param name="metaLearner">Meta learner or top level model for combining the ensemble models</param>
        /// <param name="includeOriginalFeaturesForMetaLearner">True; the meta learner also recieves the original features. 
        /// False; the meta learner only recieves the output of the ensemble models as features</param>
        public RegressionStackingEnsembleLearner(IIndexedLearner<double>[] learners, ICrossValidation<double> crossValidation,
            ILearner<double> metaLearner, bool includeOriginalFeaturesForMetaLearner)
            : this(learners, crossValidation, (obs, targets) => metaLearner.Learn(obs, targets), includeOriginalFeaturesForMetaLearner)
        {
        }

        /// <summary>
        /// Stacking Regression Ensemble Learner. 
        /// Combines several models into a single ensemble model using a top or meta level model to combine the models.
        /// </summary>
        /// <param name="learners">Learners in the ensemble</param>
        /// <param name="crossValidation">Cross validation method</param>
        /// <param name="metaLearner">Meta learner or top level model for combining the ensemble models</param>
        /// <param name="includeOriginalFeaturesForMetaLearner">True; the meta learner also recieves the original features. 
        /// False; the meta learner only recieves the output of the ensemble models as features</param>
        public RegressionStackingEnsembleLearner(IIndexedLearner<double>[] learners, ICrossValidation<double> crossValidation,
            Func<F64Matrix, double[], IPredictorModel<double>> metaLearner, bool includeOriginalFeaturesForMetaLearner)
        {
            if (learners == null) { throw new ArgumentException("learners"); }
            if (crossValidation == null) { throw new ArgumentException("crossValidation"); }
            if (metaLearner == null) { throw new ArgumentException("metaLearner"); }
            m_learners = learners;
            m_crossValidation = crossValidation;
            m_metaLearner = metaLearner;
            m_includeOriginalFeaturesForMetaLearner = includeOriginalFeaturesForMetaLearner;
        }

        /// <summary>
        /// Learns a stacking regression ensemble
        /// </summary>
        /// <param name="observations"></param>
        /// <param name="targets"></param>
        /// <returns></returns>
        public RegressionStackingEnsembleModel Learn(F64Matrix observations, double[] targets)
        {
            var indices = Enumerable.Range(0, targets.Length).ToArray();
            return Learn(observations, targets, indices);
        }

        /// <summary>
        /// Learns a stacking classification ensemble on the provided indices
        /// </summary>
        /// <param name="observations"></param>
        /// <param name="targets"></param>
        /// <param name="indices"></param>
        /// <returns></returns>
        public RegressionStackingEnsembleModel Learn(F64Matrix observations, double[] targets, int[] indices)
        {
            var metaObservations = LearnMetaFeatures(observations, targets, indices);

            var metaModelTargets = targets.GetIndices(indices);
            var metaModel = m_metaLearner(metaObservations, metaModelTargets);
            var ensembleModels = m_learners.Select(learner => learner.Learn(observations, targets, indices)).ToArray();

            return new RegressionStackingEnsembleModel(ensembleModels, metaModel, m_includeOriginalFeaturesForMetaLearner);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="observations"></param>
        /// <param name="targets"></param>
        /// <returns></returns>
        IPredictorModel<double> ILearner<double>.Learn(F64Matrix observations, double[] targets)
        {
            return Learn(observations, targets);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="observations"></param>
        /// <param name="targets"></param>
        /// <param name="indices"></param>
        /// <returns></returns>
        IPredictorModel<double> IIndexedLearner<double>.Learn(F64Matrix observations, double[] targets, int[] indices)
        {
            return Learn(observations, targets, indices);
        }

        /// <summary>
        /// Learns and extracts the meta features learned by the ensemble models
        /// </summary>
        /// <param name="observations"></param>
        /// <param name="targets"></param>
        /// <returns></returns>
        public F64Matrix LearnMetaFeatures(F64Matrix observations, double[] targets)
        {
            var indices = Enumerable.Range(0, targets.Length).ToArray();
            return LearnMetaFeatures(observations, targets, indices);
        }

        /// <summary>
        /// Learns and extracts the meta features learned by the ensemble models on the provided indices
        /// </summary>
        /// <param name="observations"></param>
        /// <param name="targets"></param>
        /// <param name="indices"></param>
        /// <returns></returns>
        public F64Matrix LearnMetaFeatures(F64Matrix observations, double[] targets, int[] indices)
        {
            var cvRows = indices.Length;
            var cvCols = m_learners.Length;

            if (m_includeOriginalFeaturesForMetaLearner)
            {
                cvCols = cvCols + observations.GetNumberOfColumns();
            }

            var cvPredictions = new F64Matrix(cvRows, cvCols);
            var modelPredictions = new double[cvRows];
            for (int i = 0; i < m_learners.Length; i++)
            {
                var learner = m_learners[i];
                m_crossValidation.CrossValidate(learner, observations, targets, indices, modelPredictions);
                for (int j = 0; j < modelPredictions.Length; j++)
                {
                    cvPredictions[j, i] = modelPredictions[j];
                }
            }

            if (m_includeOriginalFeaturesForMetaLearner)
            {
                for (int i = 0; i < cvRows; i++)
                {
                    for (int j = 0; j < observations.GetNumberOfColumns(); j++)
                    {
                        cvPredictions[i, j + m_learners.Length] = observations[indices[i], j];
                    }
                }
            }
            return cvPredictions;
        }
    }
}