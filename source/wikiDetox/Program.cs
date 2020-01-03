namespace wikiDetox
{
    using Microsoft.ML;
    using System;
    using System.IO;
    using wikiDetox.Models;

    /// <summary>
    /// Following this blog post:
    /// https://medium.com/machinelearningadvantage/filter-toxic-wikipedia-comments-with-c-and-ml-net-machine-learning-ad94869f90b4
    /// </summary>
    class Program
	{
        // filenames for data set
        private static string dataPath = Path.Combine(@".\Data\ToxicityJoinedAnnotated.tsv");

        /// <summary>
        /// The main program entry point.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        static void Main(string[] args)
        {
            // create a machine learning context
            var mlContext = new MLContext();

            // load the data file
            Console.WriteLine($"{DateTime.Now} Loading data...");
            var data = mlContext.Data.LoadFromTextFile<SentimentIssue>(dataPath, hasHeader: true);

            // split the data into 80% training and 20% testing partitions
            var partitions = mlContext.Data.TrainTestSplit(data, testFraction: 0.2);

            // build a machine learning pipeline
            // step 1: featurize the text
            var pipeline = mlContext.Transforms.Text.FeaturizeText(
                outputColumnName: "Features",
                inputColumnName: nameof(SentimentIssue.Text))

                // step 2: add a fast tree learner
                .Append(mlContext.BinaryClassification.Trainers.FastTree(
                    labelColumnName: nameof(SentimentIssue.Label),
                    featureColumnName: "Features"));

            // train the model
            Console.WriteLine($"{DateTime.Now} Training model...");
            var trainedModel = pipeline.Fit(partitions.TrainSet);

            // Save model
            mlContext.Model.Save(trainedModel, data.Schema, "model.zip");

            // Predict test data and sample one line input based on persisted model
            PredictFromModel(mlContext, partitions.TestSet, "model.zip");

/*** Sample Output:
03-Jan-20 22:47:37 Loading data...
03-Jan-20 22:47:38 Training model...
03-Jan-20 22:50:04 Evaluating model...
    Accuracy:          95.94%
    Auc:               96.68%
    Auprc:             85.73%
    F1Score:           75.35%
    LogLoss:           0.17
    LogLossReduction:  0.63
    PositivePrecision: 0.9
    PositiveRecall:    0.65
    NegativePrecision: 0.96
    NegativeRecall:    0.99

03-Jan-20 22:50:06 Making a prediction...
    Text:        With all due respect, you are a moron
    Prediction:  True
    Probability: 91.13%
    Score:       5.8247786
***/
        }

        /// <summary>
        /// Evaluates a stored models performance based on KPIs from test data
        /// and a one line sample input string.
        /// </summary>
        /// <param name="mlContext"></param>
        /// <param name="testDataSet"></param>
        /// <param name="modelSourceFileName"></param>
        private static void PredictFromModel(MLContext mlContext,
                                             IDataView testDataSet,
                                             string modelSourceFileName)
        {
            //Define DataViewSchema for data preparation pipeline and trained model
            DataViewSchema modelSchema;

            // Load trained model
            ITransformer trainedModel = mlContext.Model.Load(modelSourceFileName, out modelSchema);

            // validate the model
            Console.WriteLine($"{DateTime.Now} Evaluating model...");
            var predictions = trainedModel.Transform(testDataSet);
            var metrics = mlContext.BinaryClassification.Evaluate(
                data: predictions,
                labelColumnName: nameof(SentimentIssue.Label),
                scoreColumnName: "Score");

            // report the results
            Console.WriteLine($"  Accuracy:          {metrics.Accuracy:P2}");
            Console.WriteLine($"  Auc:               {metrics.AreaUnderRocCurve:P2}");
            Console.WriteLine($"  Auprc:             {metrics.AreaUnderPrecisionRecallCurve:P2}");
            Console.WriteLine($"  F1Score:           {metrics.F1Score:P2}");
            Console.WriteLine($"  LogLoss:           {metrics.LogLoss:0.##}");
            Console.WriteLine($"  LogLossReduction:  {metrics.LogLossReduction:0.##}");
            Console.WriteLine($"  PositivePrecision: {metrics.PositivePrecision:0.##}");
            Console.WriteLine($"  PositiveRecall:    {metrics.PositiveRecall:0.##}");
            Console.WriteLine($"  NegativePrecision: {metrics.NegativePrecision:0.##}");
            Console.WriteLine($"  NegativeRecall:    {metrics.NegativeRecall:0.##}");
            Console.WriteLine();

            // create a prediction engine to make a single prediction
            Console.WriteLine($"{DateTime.Now} Making a prediction...");
            var issue = new SentimentIssue { Text = "With all due respect, you are a moron" };
            var engine = mlContext.Model.CreatePredictionEngine<SentimentIssue, SentimentPrediction>(trainedModel);

            // make a single prediction
            var prediction = engine.Predict(issue);

            // report results
            Console.WriteLine($"  Text:        {issue.Text}");
            Console.WriteLine($"  Prediction:  {prediction.Prediction}");
            Console.WriteLine($"  Probability: {prediction.Probability:P2}");
            Console.WriteLine($"  Score:       {prediction.Score}");
        }
    }
}
