using System;
using System.Collections.Generic;
using Xbim.IO;

namespace TUM.CMS.VplControl.IFC.Utilities
{
    public sealed class DataController
    {
        // Singleton
        private static DataController _instance;
        private static readonly object Padlock = new object();
        public Dictionary<string, XbimModel> modelStorage;

        private DataController()
        {
            modelStorage = new Dictionary<string, XbimModel>();
        }

        public static DataController Instance
        {
            get
            {
                lock (Padlock)
                {
                    return _instance ?? (_instance = new DataController());
                }
            }
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="fileString"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool AddModel(string fileString, XbimModel model)
        {
            modelStorage.Add(fileString, model);
            return true;
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="fileString"></param>
        /// <returns></returns>
        public bool RemoveModel(string fileString)
        {
            modelStorage.Remove(fileString);
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public XbimModel GetModel(string filePath)
        {
            modelStorage[filePath].Open(filePath);
            return modelStorage[filePath];

        }
    }
}
