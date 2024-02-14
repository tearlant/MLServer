using Microsoft.ML.Data;
using Microsoft.ML.Transforms.Image;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Microsoft.ML.Data;
using System.Collections;

namespace DataTrainer
{
    public class ImageNetData
    {
        [LoadColumn(0)]
        public string ImagePath;

        [LoadColumn(1)]
        public string Label;

        public static IEnumerable<ImageNetData> ReadFromCsv(string file, string folder)
        {
            return File.ReadAllLines(file)
             .Select(x => x.Split('\t'))
             .Select(x => new ImageNetData { ImagePath = Path.Combine(folder, x[0]), Label = x[1] } );
        }

        public static IEnumerable<ImageNetData> LoadImagesFromDirectory(string folder, bool useFolderNameAsLabel = true)
        {
            return Directory.GetFiles(folder, "*", searchOption: SearchOption.AllDirectories)
                .Where(file => (Path.GetExtension(file) == ".jpg") || (Path.GetExtension(file) == ".png"))
                .Select(file => {
                    var label = Path.GetFileName(file);

                    if (useFolderNameAsLabel)
                        label = Directory.GetParent(file).Name;
                    else
                    {
                        for (int index = 0; index < label.Length; index++)
                        {
                            if (!char.IsLetter(label[index]))
                            {
                                label = label.Substring(0, index);
                                break;
                            }
                        }
                    }

                    return new ImageNetData()
                    {
                        ImagePath = file,
                        Label = label
                    };

                });
        }

    }

    public class ImageNetDataForTFModel
    {
        [LoadColumn(0)]
        public string ImagePath;

        [LoadColumn(1)]
        public string Label;

        [ColumnName("input")]
        [VectorType(224, 224, 3)]
        public VBuffer<Single> input { get; set; }
    }

    public class ImageNetDataProbability : ImageNetData
    {
        public string PredictedLabel;
        public float Probability { get; set; }
    }
}
