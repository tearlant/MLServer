using CommandLine;

namespace DataTrainer
{
    public class Program
    {
        public class Options
        {
            [Option('d', "data_path", Required = true, HelpText = "Path to the folder where the training data is.")]
            public string DataPath { get; set; }

            [Option('o', "output_path", Required = false, Default = "C:/Target/OutputDir", HelpText = "Path to the folder where the model ZIP file will be saved.")]
            public string OutputPath { get; set; }

            [Option('n', "prediction_points", Required = false, Default = 10, HelpText = "Number of points to predict.")]
            public int PredictionPoints { get; set; }


        }

        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args).WithParsed(options => {
                var dataPath = GetAbsolutePath(Path.Combine(@"./", options.DataPath));
                var modelsPath = GetAbsolutePath(Path.Combine(@"./", options.OutputPath));
                var outputPath = Path.Combine(modelsPath, "Model.zip");

                string assetsRelativePath = @"../../../assets";
                string assetsPath = GetAbsolutePath(assetsRelativePath);

                var inceptionPb = Path.Combine(assetsPath, "inputs", "inception", "tensorflow_inception_graph.pb");

                try
                {
                    var modelScorer = new TFModelScorer(dataPath, inceptionPb, outputPath);
                    modelScorer.FitModelAndScore(options.PredictionPoints);
                }
                catch (Exception ex)
                {
                    ConsoleHelpers.ConsoleWriteException(ex.ToString());
                }

                ConsoleHelpers.ConsolePressAnyKey();

            });

        }

        public static string GetAbsolutePath(string relativePath)
        {
            //FileInfo _dataRoot = new FileInfo(typeof(Program).Assembly.Location);
            var _dataRoot = AppDomain.CurrentDomain.BaseDirectory;
            //string assemblyFolderPath = _dataRoot.Directory.FullName;
            string fullPath = Path.Combine(_dataRoot, relativePath);
            return fullPath;
        }
    }
}
