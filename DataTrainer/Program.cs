using CommandLine;

namespace DataTrainer
{
    public class Program
    {
        public class Options
        {
            [Option('d', "data_path", Required = true, HelpText = "Path to the folder where the training data is.")]
            public string DataPath { get; set; }

            [Option('t', "tf_model_path", Required = false, HelpText = "Path to the TensorFlow file. (Defaults to included copy of tensorflow_inception_graph.pb, for now it must be a file with the same schema)")]
            public string TFPath { get; set; }

            [Option('o', "output_path", Required = false, Default = "Outputs", HelpText = "Path to the folder where the model ZIP file will be saved.")]
            public string OutputPath { get; set; }

            [Option('f', "output_filename", Required = false, Default = "Model", HelpText = "Path to the folder where the model ZIP file will be saved.")]
            public string OutputFilename { get; set; }

            [Option('n', "prediction_points", Required = false, Default = 10, HelpText = "Number of points to predict.")]
            public int PredictionPoints { get; set; }


        }

        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args).WithParsed(options => {

                var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

                var dataPath = Path.IsPathRooted(options.DataPath) ? options.DataPath : Path.Combine(baseDirectory, options.DataPath);
                var modelsPath = Path.IsPathRooted(options.OutputPath) ? options.OutputPath : Path.Combine(baseDirectory, options.OutputPath);
                var outputFilePath = Path.Combine(modelsPath, $"{options.OutputFilename}.zip");

                var defaultTensorFlowPath = Path.Combine(baseDirectory, "assets", "inputs", "inception", "tensorflow_inception_graph.pb");

                //string assetsRelativePath = @"../../../assets";
                //string assetsPath = GetAbsolutePath(assetsRelativePath);

                string tensorFlowPath;
                if (options.TFPath != null)
                {
                    tensorFlowPath = Path.IsPathRooted(options.TFPath) ? options.TFPath : Path.Combine(baseDirectory, options.TFPath);
                }
                else
                {
                    tensorFlowPath = defaultTensorFlowPath;
                }

                try
                {
                    var modelScorer = new TFModelScorer(dataPath, tensorFlowPath, outputFilePath);
                    modelScorer.FitModelAndScore(options.PredictionPoints);
                }
                catch (Exception ex)
                {
                    ConsoleHelpers.ConsoleWriteException(ex.ToString());
                }

                ConsoleHelpers.ConsolePressAnyKey();

            });
        }
    }
}
