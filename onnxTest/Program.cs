using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;


namespace onnxTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var session = new InferenceSession("path to a valid onnx model", new SessionOptions
            {
                LogSeverityLevel = OrtLoggingLevel.ORT_LOGGING_LEVEL_ERROR
            });
        }
    }
}
