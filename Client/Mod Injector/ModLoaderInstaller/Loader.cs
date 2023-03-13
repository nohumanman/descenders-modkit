using UnityEngine;
using ModTool.Interface;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using System;

namespace ModInjector
{
    public class ModInjector
    {
        public static int Load(string param)
        {
            try
            {
                SharpMonoInjector.Injector injector;
                injector = new SharpMonoInjector.Injector(Process.GetCurrentProcess().Id);
                string binPath = (
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
                    + "Low\\RageSquid\\Descenders\\ModLoaderSolution.bin"
                );
                byte[] assembly;
                assembly = File.ReadAllBytes(binPath);
                injector.Inject(assembly, "ModLoaderSolution", "Loader", "Load");
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
            return 0;
        }
    }
}

