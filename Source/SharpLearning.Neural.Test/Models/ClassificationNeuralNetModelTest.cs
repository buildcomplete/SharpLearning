﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpLearning.Containers;
using SharpLearning.Containers.Matrices;
using SharpLearning.Metrics.Classification;
using SharpLearning.Neural.Layers;
using SharpLearning.Neural.Learners;
using SharpLearning.Neural.Loss;
using SharpLearning.Neural.Models;
using System;
using System.IO;
using System.Linq;

namespace SharpLearning.Neural.Test.Models
{
    [TestClass]
    public class ClassificationNeuralNetModelTest
    {
        [TestMethod]
        public void ClassificationNeuralNetModel_Predict_Single()
        {
            var numberOfObservations = 500;
            var numberOfFeatures = 5;
            var numberOfClasses = 5;

            var random = new Random(32);
            var observations = new F64Matrix(numberOfObservations, numberOfFeatures);
            observations.Map(() => random.NextDouble());
            var targets = Enumerable.Range(0, numberOfObservations).Select(i => (double)random.Next(0, numberOfClasses)).ToArray();

            var sut = ClassificationNeuralNetModel.Load(() => new StringReader(ClassificationNeuralNetModelText));

            var predictions = new double[numberOfObservations];
            for (int i = 0; i < numberOfObservations; i++)
            {
                predictions[i] = sut.Predict(observations.Row(i));
            }

            var evaluator = new TotalErrorClassificationMetric<double>();
            var actual = evaluator.Error(targets, predictions);

            Assert.AreEqual(0.77, actual);
        }

        [TestMethod]
        public void ClassificationNeuralNetModel_Predict_Multiple()
        {
            var numberOfObservations = 500;
            var numberOfFeatures = 5;
            var numberOfClasses = 5;

            var random = new Random(32);
            var observations = new F64Matrix(numberOfObservations, numberOfFeatures);
            observations.Map(() => random.NextDouble());
            var targets = Enumerable.Range(0, numberOfObservations).Select(i => (double)random.Next(0, numberOfClasses)).ToArray();

            var sut = ClassificationNeuralNetModel.Load(() => new StringReader(ClassificationNeuralNetModelText));

            var predictions = sut.Predict(observations);

            var evaluator = new TotalErrorClassificationMetric<double>();
            var actual = evaluator.Error(targets, predictions);

            Assert.AreEqual(0.77, actual);
        }

        [TestMethod]
        public void ClassificationNeuralNetModel_PredictProbability_Single()
        {
            var numberOfObservations = 500;
            var numberOfFeatures = 5;
            var numberOfClasses = 5;

            var random = new Random(32);
            var observations = new F64Matrix(numberOfObservations, numberOfFeatures);
            observations.Map(() => random.NextDouble());
            var targets = Enumerable.Range(0, numberOfObservations).Select(i => (double)random.Next(0, numberOfClasses)).ToArray();

            var sut = ClassificationNeuralNetModel.Load(() => new StringReader(ClassificationNeuralNetModelText));

            var predictions = new ProbabilityPrediction[numberOfObservations];
            for (int i = 0; i < numberOfObservations; i++)
            {
                predictions[i] = sut.PredictProbability(observations.Row(i));
            }

            var evaluator = new TotalErrorClassificationMetric<double>();
            var actual = evaluator.Error(targets, predictions.Select(p => p.Prediction).ToArray());

            Assert.AreEqual(0.77, actual);
        }

        [TestMethod]
        public void ClassificationNeuralNetModel_PredictProbability_Multiple()
        {
            var numberOfObservations = 500;
            var numberOfFeatures = 5;
            var numberOfClasses = 5;

            var random = new Random(32);
            var observations = new F64Matrix(numberOfObservations, numberOfFeatures);
            observations.Map(() => random.NextDouble());
            var targets = Enumerable.Range(0, numberOfObservations).Select(i => (double)random.Next(0, numberOfClasses)).ToArray();

            var sut = ClassificationNeuralNetModel.Load(() => new StringReader(ClassificationNeuralNetModelText));

            var predictions = sut.PredictProbability(observations);

            var evaluator = new TotalErrorClassificationMetric<double>();
            var actual = evaluator.Error(targets, predictions.Select(p => p.Prediction).ToArray());

            Assert.AreEqual(0.77, actual);
        }

        [TestMethod]
        public void ClassificationNeuralNetModel_Save()
        {
            var numberOfObservations = 500;
            var numberOfFeatures = 5;
            var numberOfClasses = 5;

            var random = new Random(32);
            var observations = new F64Matrix(numberOfObservations, numberOfFeatures);
            observations.Map(() => random.NextDouble());
            var targets = Enumerable.Range(0, numberOfObservations).Select(i => (double)random.Next(0, numberOfClasses)).ToArray();

            var net = new NeuralNet();
            net.Add(new InputLayer(numberOfFeatures));
            net.Add(new DenseLayer(10));
            net.Add(new SvmLayer(numberOfClasses));

            var learner = new ClassificationNeuralNetLearner(net, new AccuracyLoss());
            var sut = learner.Learn(observations, targets);

            var writer = new StringWriter();
            sut.Save(() => writer);

            var actual = writer.ToString();
            Assert.AreEqual(ClassificationNeuralNetModelText, actual);
        }

        string ClassificationNeuralNetModelText = "<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n<ClassificationNeuralNetModel xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" z:Id=\"1\" xmlns:z=\"http://schemas.microsoft.com/2003/10/Serialization/\" xmlns=\"http://schemas.datacontract.org/2004/07/SharpLearning.Neural.Models\">\r\n  <m_neuralNet xmlns:d2p1=\"http://schemas.datacontract.org/2004/07/SharpLearning.Neural\" z:Id=\"2\">\r\n    <d2p1:Layers xmlns:d3p1=\"http://schemas.microsoft.com/2003/10/Serialization/Arrays\" z:Id=\"3\" z:Size=\"5\">\r\n      <d3p1:anyType z:Id=\"4\" xmlns:d4p1=\"SharpLearning.Neural.Layers\" i:type=\"d4p1:InputLayer\">\r\n        <_x003C_ActivationFunc_x003E_k__BackingField xmlns=\"http://schemas.datacontract.org/2004/07/SharpLearning.Neural.Layers\">Undefined</_x003C_ActivationFunc_x003E_k__BackingField>\r\n        <_x003C_Depth_x003E_k__BackingField xmlns=\"http://schemas.datacontract.org/2004/07/SharpLearning.Neural.Layers\">5</_x003C_Depth_x003E_k__BackingField>\r\n        <_x003C_Height_x003E_k__BackingField xmlns=\"http://schemas.datacontract.org/2004/07/SharpLearning.Neural.Layers\">1</_x003C_Height_x003E_k__BackingField>\r\n        <_x003C_Width_x003E_k__BackingField xmlns=\"http://schemas.datacontract.org/2004/07/SharpLearning.Neural.Layers\">1</_x003C_Width_x003E_k__BackingField>\r\n      </d3p1:anyType>\r\n      <d3p1:anyType z:Id=\"5\" xmlns:d4p1=\"SharpLearning.Neural.Layers\" i:type=\"d4p1:DenseLayer\">\r\n        <Bias xmlns:d5p1=\"http://schemas.datacontract.org/2004/07/MathNet.Numerics.LinearAlgebra\" z:Id=\"6\" xmlns:d5p2=\"MathNet.Numerics.LinearAlgebra.Single\" i:type=\"d5p2:DenseVector\" xmlns=\"http://schemas.datacontract.org/2004/07/SharpLearning.Neural.Layers\">\r\n          <d5p1:_x003C_Count_x003E_k__BackingField>10</d5p1:_x003C_Count_x003E_k__BackingField>\r\n          <d5p1:_x003C_Storage_x003E_k__BackingField xmlns:d6p1=\"urn:MathNet/Numerics/LinearAlgebra\" z:Id=\"7\" i:type=\"d6p1:DenseVectorStorageOffloat\">\r\n            <d6p1:Length>10</d6p1:Length>\r\n            <d6p1:Data z:Id=\"8\" z:Size=\"10\">\r\n              <d3p1:float>0.0815593153</d3p1:float>\r\n              <d3p1:float>-0.0952108055</d3p1:float>\r\n              <d3p1:float>0.0386274047</d3p1:float>\r\n              <d3p1:float>-0.12638998</d3p1:float>\r\n              <d3p1:float>0.0152520742</d3p1:float>\r\n              <d3p1:float>0.07557311</d3p1:float>\r\n              <d3p1:float>0.0499726124</d3p1:float>\r\n              <d3p1:float>-0.0164522789</d3p1:float>\r\n              <d3p1:float>0.06344382</d3p1:float>\r\n              <d3p1:float>0.0393575132</d3p1:float>\r\n            </d6p1:Data>\r\n          </d5p1:_x003C_Storage_x003E_k__BackingField>\r\n          <_length xmlns=\"http://schemas.datacontract.org/2004/07/MathNet.Numerics.LinearAlgebra.Single\">10</_length>\r\n          <_values z:Ref=\"8\" i:nil=\"true\" xmlns=\"http://schemas.datacontract.org/2004/07/MathNet.Numerics.LinearAlgebra.Single\" />\r\n        </Bias>\r\n        <BiasGradients xmlns:d5p1=\"http://schemas.datacontract.org/2004/07/MathNet.Numerics.LinearAlgebra\" i:nil=\"true\" xmlns=\"http://schemas.datacontract.org/2004/07/SharpLearning.Neural.Layers\" />\r\n        <OutputActivations xmlns:d5p1=\"http://schemas.datacontract.org/2004/07/MathNet.Numerics.LinearAlgebra\" z:Id=\"9\" xmlns:d5p2=\"MathNet.Numerics.LinearAlgebra.Single\" i:type=\"d5p2:DenseMatrix\" xmlns=\"http://schemas.datacontract.org/2004/07/SharpLearning.Neural.Layers\">\r\n          <d5p1:_x003C_ColumnCount_x003E_k__BackingField>10</d5p1:_x003C_ColumnCount_x003E_k__BackingField>\r\n          <d5p1:_x003C_RowCount_x003E_k__BackingField>1</d5p1:_x003C_RowCount_x003E_k__BackingField>\r\n          <d5p1:_x003C_Storage_x003E_k__BackingField xmlns:d6p1=\"urn:MathNet/Numerics/LinearAlgebra\" z:Id=\"10\" i:type=\"d6p1:DenseColumnMajorMatrixStorageOffloat\">\r\n            <d6p1:RowCount>1</d6p1:RowCount>\r\n            <d6p1:ColumnCount>10</d6p1:ColumnCount>\r\n            <d6p1:Data z:Id=\"11\" z:Size=\"10\">\r\n              <d3p1:float>0</d3p1:float>\r\n              <d3p1:float>0</d3p1:float>\r\n              <d3p1:float>0</d3p1:float>\r\n              <d3p1:float>0</d3p1:float>\r\n              <d3p1:float>0</d3p1:float>\r\n              <d3p1:float>0</d3p1:float>\r\n              <d3p1:float>0</d3p1:float>\r\n              <d3p1:float>0</d3p1:float>\r\n              <d3p1:float>0</d3p1:float>\r\n              <d3p1:float>0</d3p1:float>\r\n            </d6p1:Data>\r\n          </d5p1:_x003C_Storage_x003E_k__BackingField>\r\n          <_columnCount xmlns=\"http://schemas.datacontract.org/2004/07/MathNet.Numerics.LinearAlgebra.Single\">10</_columnCount>\r\n          <_rowCount xmlns=\"http://schemas.datacontract.org/2004/07/MathNet.Numerics.LinearAlgebra.Single\">1</_rowCount>\r\n          <_values z:Ref=\"11\" i:nil=\"true\" xmlns=\"http://schemas.datacontract.org/2004/07/MathNet.Numerics.LinearAlgebra.Single\" />\r\n        </OutputActivations>\r\n        <Weights xmlns:d5p1=\"http://schemas.datacontract.org/2004/07/MathNet.Numerics.LinearAlgebra\" z:Id=\"12\" xmlns:d5p2=\"MathNet.Numerics.LinearAlgebra.Single\" i:type=\"d5p2:DenseMatrix\" xmlns=\"http://schemas.datacontract.org/2004/07/SharpLearning.Neural.Layers\">\r\n          <d5p1:_x003C_ColumnCount_x003E_k__BackingField>10</d5p1:_x003C_ColumnCount_x003E_k__BackingField>\r\n          <d5p1:_x003C_RowCount_x003E_k__BackingField>5</d5p1:_x003C_RowCount_x003E_k__BackingField>\r\n          <d5p1:_x003C_Storage_x003E_k__BackingField xmlns:d6p1=\"urn:MathNet/Numerics/LinearAlgebra\" z:Id=\"13\" i:type=\"d6p1:DenseColumnMajorMatrixStorageOffloat\">\r\n            <d6p1:RowCount>5</d6p1:RowCount>\r\n            <d6p1:ColumnCount>10</d6p1:ColumnCount>\r\n            <d6p1:Data z:Id=\"14\" z:Size=\"50\">\r\n              <d3p1:float>0.185616285</d3p1:float>\r\n              <d3p1:float>0.382907033</d3p1:float>\r\n              <d3p1:float>0.08028838</d3p1:float>\r\n              <d3p1:float>0.07914696</d3p1:float>\r\n              <d3p1:float>0.482129216</d3p1:float>\r\n              <d3p1:float>-0.173369</d3p1:float>\r\n              <d3p1:float>0.06927832</d3p1:float>\r\n              <d3p1:float>-0.484601438</d3p1:float>\r\n              <d3p1:float>0.2928645</d3p1:float>\r\n              <d3p1:float>-0.21685487</d3p1:float>\r\n              <d3p1:float>0.0384059064</d3p1:float>\r\n              <d3p1:float>-0.424248964</d3p1:float>\r\n              <d3p1:float>0.374701679</d3p1:float>\r\n              <d3p1:float>0.08440649</d3p1:float>\r\n              <d3p1:float>0.1219281</d3p1:float>\r\n              <d3p1:float>-0.193348974</d3p1:float>\r\n              <d3p1:float>0.0444230437</d3p1:float>\r\n              <d3p1:float>-0.4113923</d3p1:float>\r\n              <d3p1:float>0.171179429</d3p1:float>\r\n              <d3p1:float>0.0446322858</d3p1:float>\r\n              <d3p1:float>0.199539989</d3p1:float>\r\n              <d3p1:float>-0.140713975</d3p1:float>\r\n              <d3p1:float>0.364116728</d3p1:float>\r\n              <d3p1:float>-0.0112000722</d3p1:float>\r\n              <d3p1:float>-0.102835856</d3p1:float>\r\n              <d3p1:float>-0.120500341</d3p1:float>\r\n              <d3p1:float>0.146669045</d3p1:float>\r\n              <d3p1:float>0.22517623</d3p1:float>\r\n              <d3p1:float>0.0615868643</d3p1:float>\r\n              <d3p1:float>-0.3191523</d3p1:float>\r\n              <d3p1:float>-0.15068984</d3p1:float>\r\n              <d3p1:float>0.1974303</d3p1:float>\r\n              <d3p1:float>0.224056259</d3p1:float>\r\n              <d3p1:float>0.308570743</d3p1:float>\r\n              <d3p1:float>0.3531552</d3p1:float>\r\n              <d3p1:float>0.207379192</d3p1:float>\r\n              <d3p1:float>-0.268649</d3p1:float>\r\n              <d3p1:float>-0.235272</d3p1:float>\r\n              <d3p1:float>-0.216575876</d3p1:float>\r\n              <d3p1:float>-0.0717269257</d3p1:float>\r\n              <d3p1:float>0.502353966</d3p1:float>\r\n              <d3p1:float>-0.08437626</d3p1:float>\r\n              <d3p1:float>-0.456475645</d3p1:float>\r\n              <d3p1:float>-0.0844684839</d3p1:float>\r\n              <d3p1:float>-0.166279361</d3p1:float>\r\n              <d3p1:float>-0.14220731</d3p1:float>\r\n              <d3p1:float>-0.4034073</d3p1:float>\r\n              <d3p1:float>0.551665545</d3p1:float>\r\n              <d3p1:float>0.151788071</d3p1:float>\r\n              <d3p1:float>-0.0111414855</d3p1:float>\r\n            </d6p1:Data>\r\n          </d5p1:_x003C_Storage_x003E_k__BackingField>\r\n          <_columnCount xmlns=\"http://schemas.datacontract.org/2004/07/MathNet.Numerics.LinearAlgebra.Single\">10</_columnCount>\r\n          <_rowCount xmlns=\"http://schemas.datacontract.org/2004/07/MathNet.Numerics.LinearAlgebra.Single\">5</_rowCount>\r\n          <_values z:Ref=\"14\" i:nil=\"true\" xmlns=\"http://schemas.datacontract.org/2004/07/MathNet.Numerics.LinearAlgebra.Single\" />\r\n        </Weights>\r\n        <WeightsGradients xmlns:d5p1=\"http://schemas.datacontract.org/2004/07/MathNet.Numerics.LinearAlgebra\" i:nil=\"true\" xmlns=\"http://schemas.datacontract.org/2004/07/SharpLearning.Neural.Layers\" />\r\n        <_x003C_ActivationFunc_x003E_k__BackingField xmlns=\"http://schemas.datacontract.org/2004/07/SharpLearning.Neural.Layers\">Relu</_x003C_ActivationFunc_x003E_k__BackingField>\r\n        <_x003C_Depth_x003E_k__BackingField xmlns=\"http://schemas.datacontract.org/2004/07/SharpLearning.Neural.Layers\">10</_x003C_Depth_x003E_k__BackingField>\r\n        <_x003C_Height_x003E_k__BackingField xmlns=\"http://schemas.datacontract.org/2004/07/SharpLearning.Neural.Layers\">1</_x003C_Height_x003E_k__BackingField>\r\n        <_x003C_UseBatchNormalization_x003E_k__BackingField xmlns=\"http://schemas.datacontract.org/2004/07/SharpLearning.Neural.Layers\">false</_x003C_UseBatchNormalization_x003E_k__BackingField>\r\n        <_x003C_Width_x003E_k__BackingField xmlns=\"http://schemas.datacontract.org/2004/07/SharpLearning.Neural.Layers\">1</_x003C_Width_x003E_k__BackingField>\r\n        <m_delta xmlns:d5p1=\"http://schemas.datacontract.org/2004/07/MathNet.Numerics.LinearAlgebra\" i:nil=\"true\" xmlns=\"http://schemas.datacontract.org/2004/07/SharpLearning.Neural.Layers\" />\r\n        <m_inputActivations xmlns:d5p1=\"http://schemas.datacontract.org/2004/07/MathNet.Numerics.LinearAlgebra\" i:nil=\"true\" xmlns=\"http://schemas.datacontract.org/2004/07/SharpLearning.Neural.Layers\" />\r\n      </d3p1:anyType>\r\n      <d3p1:anyType z:Id=\"15\" xmlns:d4p1=\"SharpLearning.Neural.Layers\" i:type=\"d4p1:ActivationLayer\">\r\n        <ActivationDerivative xmlns:d5p1=\"http://schemas.datacontract.org/2004/07/MathNet.Numerics.LinearAlgebra\" z:Id=\"16\" xmlns:d5p2=\"MathNet.Numerics.LinearAlgebra.Single\" i:type=\"d5p2:DenseMatrix\" xmlns=\"http://schemas.datacontract.org/2004/07/SharpLearning.Neural.Layers\">\r\n          <d5p1:_x003C_ColumnCount_x003E_k__BackingField>10</d5p1:_x003C_ColumnCount_x003E_k__BackingField>\r\n          <d5p1:_x003C_RowCount_x003E_k__BackingField>1</d5p1:_x003C_RowCount_x003E_k__BackingField>\r\n          <d5p1:_x003C_Storage_x003E_k__BackingField xmlns:d6p1=\"urn:MathNet/Numerics/LinearAlgebra\" z:Id=\"17\" i:type=\"d6p1:DenseColumnMajorMatrixStorageOffloat\">\r\n            <d6p1:RowCount>1</d6p1:RowCount>\r\n            <d6p1:ColumnCount>10</d6p1:ColumnCount>\r\n            <d6p1:Data z:Id=\"18\" z:Size=\"10\">\r\n              <d3p1:float>0</d3p1:float>\r\n              <d3p1:float>0</d3p1:float>\r\n              <d3p1:float>0</d3p1:float>\r\n              <d3p1:float>0</d3p1:float>\r\n              <d3p1:float>0</d3p1:float>\r\n              <d3p1:float>0</d3p1:float>\r\n              <d3p1:float>0</d3p1:float>\r\n              <d3p1:float>0</d3p1:float>\r\n              <d3p1:float>0</d3p1:float>\r\n              <d3p1:float>0</d3p1:float>\r\n            </d6p1:Data>\r\n          </d5p1:_x003C_Storage_x003E_k__BackingField>\r\n          <_columnCount xmlns=\"http://schemas.datacontract.org/2004/07/MathNet.Numerics.LinearAlgebra.Single\">10</_columnCount>\r\n          <_rowCount xmlns=\"http://schemas.datacontract.org/2004/07/MathNet.Numerics.LinearAlgebra.Single\">1</_rowCount>\r\n          <_values z:Ref=\"18\" i:nil=\"true\" xmlns=\"http://schemas.datacontract.org/2004/07/MathNet.Numerics.LinearAlgebra.Single\" />\r\n        </ActivationDerivative>\r\n        <OutputActivations xmlns:d5p1=\"http://schemas.datacontract.org/2004/07/MathNet.Numerics.LinearAlgebra\" z:Id=\"19\" xmlns:d5p2=\"MathNet.Numerics.LinearAlgebra.Single\" i:type=\"d5p2:DenseMatrix\" xmlns=\"http://schemas.datacontract.org/2004/07/SharpLearning.Neural.Layers\">\r\n          <d5p1:_x003C_ColumnCount_x003E_k__BackingField>10</d5p1:_x003C_ColumnCount_x003E_k__BackingField>\r\n          <d5p1:_x003C_RowCount_x003E_k__BackingField>1</d5p1:_x003C_RowCount_x003E_k__BackingField>\r\n          <d5p1:_x003C_Storage_x003E_k__BackingField xmlns:d6p1=\"urn:MathNet/Numerics/LinearAlgebra\" z:Id=\"20\" i:type=\"d6p1:DenseColumnMajorMatrixStorageOffloat\">\r\n            <d6p1:RowCount>1</d6p1:RowCount>\r\n            <d6p1:ColumnCount>10</d6p1:ColumnCount>\r\n            <d6p1:Data z:Id=\"21\" z:Size=\"10\">\r\n              <d3p1:float>0</d3p1:float>\r\n              <d3p1:float>0</d3p1:float>\r\n              <d3p1:float>0</d3p1:float>\r\n              <d3p1:float>0</d3p1:float>\r\n              <d3p1:float>0</d3p1:float>\r\n              <d3p1:float>0</d3p1:float>\r\n              <d3p1:float>0</d3p1:float>\r\n              <d3p1:float>0</d3p1:float>\r\n              <d3p1:float>0</d3p1:float>\r\n              <d3p1:float>0</d3p1:float>\r\n            </d6p1:Data>\r\n          </d5p1:_x003C_Storage_x003E_k__BackingField>\r\n          <_columnCount xmlns=\"http://schemas.datacontract.org/2004/07/MathNet.Numerics.LinearAlgebra.Single\">10</_columnCount>\r\n          <_rowCount xmlns=\"http://schemas.datacontract.org/2004/07/MathNet.Numerics.LinearAlgebra.Single\">1</_rowCount>\r\n          <_values z:Ref=\"21\" i:nil=\"true\" xmlns=\"http://schemas.datacontract.org/2004/07/MathNet.Numerics.LinearAlgebra.Single\" />\r\n        </OutputActivations>\r\n        <_x003C_ActivationFunc_x003E_k__BackingField xmlns=\"http://schemas.datacontract.org/2004/07/SharpLearning.Neural.Layers\">Relu</_x003C_ActivationFunc_x003E_k__BackingField>\r\n        <_x003C_Depth_x003E_k__BackingField xmlns=\"http://schemas.datacontract.org/2004/07/SharpLearning.Neural.Layers\">10</_x003C_Depth_x003E_k__BackingField>\r\n        <_x003C_Height_x003E_k__BackingField xmlns=\"http://schemas.datacontract.org/2004/07/SharpLearning.Neural.Layers\">1</_x003C_Height_x003E_k__BackingField>\r\n        <_x003C_Width_x003E_k__BackingField xmlns=\"http://schemas.datacontract.org/2004/07/SharpLearning.Neural.Layers\">1</_x003C_Width_x003E_k__BackingField>\r\n        <m_activation z:Id=\"22\" xmlns:d5p1=\"SharpLearning.Neural.Activations\" i:type=\"d5p1:ReluActivation\" xmlns=\"http://schemas.datacontract.org/2004/07/SharpLearning.Neural.Layers\" />\r\n        <m_delta xmlns:d5p1=\"http://schemas.datacontract.org/2004/07/MathNet.Numerics.LinearAlgebra\" i:nil=\"true\" xmlns=\"http://schemas.datacontract.org/2004/07/SharpLearning.Neural.Layers\" />\r\n        <m_inputActivations xmlns:d5p1=\"http://schemas.datacontract.org/2004/07/MathNet.Numerics.LinearAlgebra\" i:nil=\"true\" xmlns=\"http://schemas.datacontract.org/2004/07/SharpLearning.Neural.Layers\" />\r\n      </d3p1:anyType>\r\n      <d3p1:anyType z:Id=\"23\" xmlns:d4p1=\"SharpLearning.Neural.Layers\" i:type=\"d4p1:DenseLayer\">\r\n        <Bias xmlns:d5p1=\"http://schemas.datacontract.org/2004/07/MathNet.Numerics.LinearAlgebra\" z:Id=\"24\" xmlns:d5p2=\"MathNet.Numerics.LinearAlgebra.Single\" i:type=\"d5p2:DenseVector\" xmlns=\"http://schemas.datacontract.org/2004/07/SharpLearning.Neural.Layers\">\r\n          <d5p1:_x003C_Count_x003E_k__BackingField>5</d5p1:_x003C_Count_x003E_k__BackingField>\r\n          <d5p1:_x003C_Storage_x003E_k__BackingField xmlns:d6p1=\"urn:MathNet/Numerics/LinearAlgebra\" z:Id=\"25\" i:type=\"d6p1:DenseVectorStorageOffloat\">\r\n            <d6p1:Length>5</d6p1:Length>\r\n            <d6p1:Data z:Id=\"26\" z:Size=\"5\">\r\n              <d3p1:float>0.03999462</d3p1:float>\r\n              <d3p1:float>0.08200391</d3p1:float>\r\n              <d3p1:float>-0.0461643934</d3p1:float>\r\n              <d3p1:float>-0.0832364261</d3p1:float>\r\n              <d3p1:float>-0.0123899</d3p1:float>\r\n            </d6p1:Data>\r\n          </d5p1:_x003C_Storage_x003E_k__BackingField>\r\n          <_length xmlns=\"http://schemas.datacontract.org/2004/07/MathNet.Numerics.LinearAlgebra.Single\">5</_length>\r\n          <_values z:Ref=\"26\" i:nil=\"true\" xmlns=\"http://schemas.datacontract.org/2004/07/MathNet.Numerics.LinearAlgebra.Single\" />\r\n        </Bias>\r\n        <BiasGradients xmlns:d5p1=\"http://schemas.datacontract.org/2004/07/MathNet.Numerics.LinearAlgebra\" i:nil=\"true\" xmlns=\"http://schemas.datacontract.org/2004/07/SharpLearning.Neural.Layers\" />\r\n        <OutputActivations xmlns:d5p1=\"http://schemas.datacontract.org/2004/07/MathNet.Numerics.LinearAlgebra\" z:Id=\"27\" xmlns:d5p2=\"MathNet.Numerics.LinearAlgebra.Single\" i:type=\"d5p2:DenseMatrix\" xmlns=\"http://schemas.datacontract.org/2004/07/SharpLearning.Neural.Layers\">\r\n          <d5p1:_x003C_ColumnCount_x003E_k__BackingField>5</d5p1:_x003C_ColumnCount_x003E_k__BackingField>\r\n          <d5p1:_x003C_RowCount_x003E_k__BackingField>1</d5p1:_x003C_RowCount_x003E_k__BackingField>\r\n          <d5p1:_x003C_Storage_x003E_k__BackingField xmlns:d6p1=\"urn:MathNet/Numerics/LinearAlgebra\" z:Id=\"28\" i:type=\"d6p1:DenseColumnMajorMatrixStorageOffloat\">\r\n            <d6p1:RowCount>1</d6p1:RowCount>\r\n            <d6p1:ColumnCount>5</d6p1:ColumnCount>\r\n            <d6p1:Data z:Id=\"29\" z:Size=\"5\">\r\n              <d3p1:float>0</d3p1:float>\r\n              <d3p1:float>0</d3p1:float>\r\n              <d3p1:float>0</d3p1:float>\r\n              <d3p1:float>0</d3p1:float>\r\n              <d3p1:float>0</d3p1:float>\r\n            </d6p1:Data>\r\n          </d5p1:_x003C_Storage_x003E_k__BackingField>\r\n          <_columnCount xmlns=\"http://schemas.datacontract.org/2004/07/MathNet.Numerics.LinearAlgebra.Single\">5</_columnCount>\r\n          <_rowCount xmlns=\"http://schemas.datacontract.org/2004/07/MathNet.Numerics.LinearAlgebra.Single\">1</_rowCount>\r\n          <_values z:Ref=\"29\" i:nil=\"true\" xmlns=\"http://schemas.datacontract.org/2004/07/MathNet.Numerics.LinearAlgebra.Single\" />\r\n        </OutputActivations>\r\n        <Weights xmlns:d5p1=\"http://schemas.datacontract.org/2004/07/MathNet.Numerics.LinearAlgebra\" z:Id=\"30\" xmlns:d5p2=\"MathNet.Numerics.LinearAlgebra.Single\" i:type=\"d5p2:DenseMatrix\" xmlns=\"http://schemas.datacontract.org/2004/07/SharpLearning.Neural.Layers\">\r\n          <d5p1:_x003C_ColumnCount_x003E_k__BackingField>5</d5p1:_x003C_ColumnCount_x003E_k__BackingField>\r\n          <d5p1:_x003C_RowCount_x003E_k__BackingField>10</d5p1:_x003C_RowCount_x003E_k__BackingField>\r\n          <d5p1:_x003C_Storage_x003E_k__BackingField xmlns:d6p1=\"urn:MathNet/Numerics/LinearAlgebra\" z:Id=\"31\" i:type=\"d6p1:DenseColumnMajorMatrixStorageOffloat\">\r\n            <d6p1:RowCount>10</d6p1:RowCount>\r\n            <d6p1:ColumnCount>5</d6p1:ColumnCount>\r\n            <d6p1:Data z:Id=\"32\" z:Size=\"50\">\r\n              <d3p1:float>0.241281867</d3p1:float>\r\n              <d3p1:float>0.3836927</d3p1:float>\r\n              <d3p1:float>0.114363864</d3p1:float>\r\n              <d3p1:float>0.479667038</d3p1:float>\r\n              <d3p1:float>-0.043304354</d3p1:float>\r\n              <d3p1:float>0.227759</d3p1:float>\r\n              <d3p1:float>-0.504285753</d3p1:float>\r\n              <d3p1:float>-0.316325068</d3p1:float>\r\n              <d3p1:float>0.647149444</d3p1:float>\r\n              <d3p1:float>0.2787304</d3p1:float>\r\n              <d3p1:float>0.375847429</d3p1:float>\r\n              <d3p1:float>0.173139855</d3p1:float>\r\n              <d3p1:float>-0.500365436</d3p1:float>\r\n              <d3p1:float>-0.140053347</d3p1:float>\r\n              <d3p1:float>0.582303643</d3p1:float>\r\n              <d3p1:float>0.237245351</d3p1:float>\r\n              <d3p1:float>0.6419558</d3p1:float>\r\n              <d3p1:float>0.006917404</d3p1:float>\r\n              <d3p1:float>0.2592402</d3p1:float>\r\n              <d3p1:float>-0.307307422</d3p1:float>\r\n              <d3p1:float>-0.0915959254</d3p1:float>\r\n              <d3p1:float>0.10195598</d3p1:float>\r\n              <d3p1:float>-0.3066638</d3p1:float>\r\n              <d3p1:float>0.5232214</d3p1:float>\r\n              <d3p1:float>0.310122073</d3p1:float>\r\n              <d3p1:float>-0.212076023</d3p1:float>\r\n              <d3p1:float>0.0181317572</d3p1:float>\r\n              <d3p1:float>0.331821918</d3p1:float>\r\n              <d3p1:float>0.100406684</d3p1:float>\r\n              <d3p1:float>0.127332091</d3p1:float>\r\n              <d3p1:float>0.157990575</d3p1:float>\r\n              <d3p1:float>0.398043334</d3p1:float>\r\n              <d3p1:float>-0.487679541</d3p1:float>\r\n              <d3p1:float>0.47108084</d3p1:float>\r\n              <d3p1:float>0.0503913872</d3p1:float>\r\n              <d3p1:float>-0.336538047</d3p1:float>\r\n              <d3p1:float>-0.216933429</d3p1:float>\r\n              <d3p1:float>0.4208404</d3p1:float>\r\n              <d3p1:float>0.0574047565</d3p1:float>\r\n              <d3p1:float>-0.0110807214</d3p1:float>\r\n              <d3p1:float>-0.04392361</d3p1:float>\r\n              <d3p1:float>0.431027025</d3p1:float>\r\n              <d3p1:float>-0.375582248</d3p1:float>\r\n              <d3p1:float>-0.125146151</d3p1:float>\r\n              <d3p1:float>-0.632948637</d3p1:float>\r\n              <d3p1:float>-0.2514647</d3p1:float>\r\n              <d3p1:float>0.262171924</d3p1:float>\r\n              <d3p1:float>-0.392840028</d3p1:float>\r\n              <d3p1:float>-0.719291151</d3p1:float>\r\n              <d3p1:float>-0.490308553</d3p1:float>\r\n            </d6p1:Data>\r\n          </d5p1:_x003C_Storage_x003E_k__BackingField>\r\n          <_columnCount xmlns=\"http://schemas.datacontract.org/2004/07/MathNet.Numerics.LinearAlgebra.Single\">5</_columnCount>\r\n          <_rowCount xmlns=\"http://schemas.datacontract.org/2004/07/MathNet.Numerics.LinearAlgebra.Single\">10</_rowCount>\r\n          <_values z:Ref=\"32\" i:nil=\"true\" xmlns=\"http://schemas.datacontract.org/2004/07/MathNet.Numerics.LinearAlgebra.Single\" />\r\n        </Weights>\r\n        <WeightsGradients xmlns:d5p1=\"http://schemas.datacontract.org/2004/07/MathNet.Numerics.LinearAlgebra\" i:nil=\"true\" xmlns=\"http://schemas.datacontract.org/2004/07/SharpLearning.Neural.Layers\" />\r\n        <_x003C_ActivationFunc_x003E_k__BackingField xmlns=\"http://schemas.datacontract.org/2004/07/SharpLearning.Neural.Layers\">Undefined</_x003C_ActivationFunc_x003E_k__BackingField>\r\n        <_x003C_Depth_x003E_k__BackingField xmlns=\"http://schemas.datacontract.org/2004/07/SharpLearning.Neural.Layers\">5</_x003C_Depth_x003E_k__BackingField>\r\n        <_x003C_Height_x003E_k__BackingField xmlns=\"http://schemas.datacontract.org/2004/07/SharpLearning.Neural.Layers\">1</_x003C_Height_x003E_k__BackingField>\r\n        <_x003C_UseBatchNormalization_x003E_k__BackingField xmlns=\"http://schemas.datacontract.org/2004/07/SharpLearning.Neural.Layers\">false</_x003C_UseBatchNormalization_x003E_k__BackingField>\r\n        <_x003C_Width_x003E_k__BackingField xmlns=\"http://schemas.datacontract.org/2004/07/SharpLearning.Neural.Layers\">1</_x003C_Width_x003E_k__BackingField>\r\n        <m_delta xmlns:d5p1=\"http://schemas.datacontract.org/2004/07/MathNet.Numerics.LinearAlgebra\" i:nil=\"true\" xmlns=\"http://schemas.datacontract.org/2004/07/SharpLearning.Neural.Layers\" />\r\n        <m_inputActivations xmlns:d5p1=\"http://schemas.datacontract.org/2004/07/MathNet.Numerics.LinearAlgebra\" i:nil=\"true\" xmlns=\"http://schemas.datacontract.org/2004/07/SharpLearning.Neural.Layers\" />\r\n      </d3p1:anyType>\r\n      <d3p1:anyType z:Id=\"33\" xmlns:d4p1=\"SharpLearning.Neural.Layers\" i:type=\"d4p1:SvmLayer\">\r\n        <NumberOfClasses xmlns=\"http://schemas.datacontract.org/2004/07/SharpLearning.Neural.Layers\">5</NumberOfClasses>\r\n        <OutputActivations xmlns:d5p1=\"http://schemas.datacontract.org/2004/07/MathNet.Numerics.LinearAlgebra\" z:Id=\"34\" xmlns:d5p2=\"MathNet.Numerics.LinearAlgebra.Single\" i:type=\"d5p2:DenseMatrix\" xmlns=\"http://schemas.datacontract.org/2004/07/SharpLearning.Neural.Layers\">\r\n          <d5p1:_x003C_ColumnCount_x003E_k__BackingField>5</d5p1:_x003C_ColumnCount_x003E_k__BackingField>\r\n          <d5p1:_x003C_RowCount_x003E_k__BackingField>1</d5p1:_x003C_RowCount_x003E_k__BackingField>\r\n          <d5p1:_x003C_Storage_x003E_k__BackingField xmlns:d6p1=\"urn:MathNet/Numerics/LinearAlgebra\" z:Id=\"35\" i:type=\"d6p1:DenseColumnMajorMatrixStorageOffloat\">\r\n            <d6p1:RowCount>1</d6p1:RowCount>\r\n            <d6p1:ColumnCount>5</d6p1:ColumnCount>\r\n            <d6p1:Data z:Id=\"36\" z:Size=\"5\">\r\n              <d3p1:float>0</d3p1:float>\r\n              <d3p1:float>0</d3p1:float>\r\n              <d3p1:float>0</d3p1:float>\r\n              <d3p1:float>0</d3p1:float>\r\n              <d3p1:float>0</d3p1:float>\r\n            </d6p1:Data>\r\n          </d5p1:_x003C_Storage_x003E_k__BackingField>\r\n          <_columnCount xmlns=\"http://schemas.datacontract.org/2004/07/MathNet.Numerics.LinearAlgebra.Single\">5</_columnCount>\r\n          <_rowCount xmlns=\"http://schemas.datacontract.org/2004/07/MathNet.Numerics.LinearAlgebra.Single\">1</_rowCount>\r\n          <_values z:Ref=\"36\" i:nil=\"true\" xmlns=\"http://schemas.datacontract.org/2004/07/MathNet.Numerics.LinearAlgebra.Single\" />\r\n        </OutputActivations>\r\n        <_x003C_ActivationFunc_x003E_k__BackingField xmlns=\"http://schemas.datacontract.org/2004/07/SharpLearning.Neural.Layers\">Undefined</_x003C_ActivationFunc_x003E_k__BackingField>\r\n        <_x003C_Depth_x003E_k__BackingField xmlns=\"http://schemas.datacontract.org/2004/07/SharpLearning.Neural.Layers\">5</_x003C_Depth_x003E_k__BackingField>\r\n        <_x003C_Height_x003E_k__BackingField xmlns=\"http://schemas.datacontract.org/2004/07/SharpLearning.Neural.Layers\">1</_x003C_Height_x003E_k__BackingField>\r\n        <_x003C_Width_x003E_k__BackingField xmlns=\"http://schemas.datacontract.org/2004/07/SharpLearning.Neural.Layers\">1</_x003C_Width_x003E_k__BackingField>\r\n        <m_delta xmlns:d5p1=\"http://schemas.datacontract.org/2004/07/MathNet.Numerics.LinearAlgebra\" i:nil=\"true\" xmlns=\"http://schemas.datacontract.org/2004/07/SharpLearning.Neural.Layers\" />\r\n      </d3p1:anyType>\r\n    </d2p1:Layers>\r\n  </m_neuralNet>\r\n  <m_targetNames xmlns:d2p1=\"http://schemas.microsoft.com/2003/10/Serialization/Arrays\" z:Id=\"37\" z:Size=\"5\">\r\n    <d2p1:double>0</d2p1:double>\r\n    <d2p1:double>1</d2p1:double>\r\n    <d2p1:double>2</d2p1:double>\r\n    <d2p1:double>3</d2p1:double>\r\n    <d2p1:double>4</d2p1:double>\r\n  </m_targetNames>\r\n</ClassificationNeuralNetModel>";
    }
}
