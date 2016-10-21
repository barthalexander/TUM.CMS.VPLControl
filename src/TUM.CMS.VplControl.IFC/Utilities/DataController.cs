using System;
using System.Collections.Generic;
using Xbim.Common;
using Xbim.Ifc;
using Xbim.IO;
using Xbim.IO.Esent;
using Xbim.XbimExtensions;

namespace TUM.CMS.VplControl.IFC.Utilities
{
    public sealed class DataController
    {
        // Singleton
        private static DataController _instance;
        private static readonly object Padlock = new object();
        public Dictionary<string, IfcStore> ModelStorage;

        private DataController()
        {
            ModelStorage = new Dictionary<string, IfcStore>();
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
        /// Models will be added to the DataController.
        /// 
        /// GUID is the file Path.
        /// </summary>
        /// <param name="fileString"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool AddModel(string fileString, IfcStore model)
        {
            ModelStorage.Add(fileString, model);
            return true;
        }

        /// <summary>
        /// Models will be removed from the DataController.
        /// 
        /// Benefit: No big DataController
        /// </summary>
        /// <param name="fileString"></param>
        /// <returns></returns>
        public bool RemoveModel(string fileString)
        {
            ModelStorage.Remove(fileString);
            return true;
        }

        /// <summary>
        /// Reading / Opening the xBIM File. 
        /// 
        /// This Method must be implemented in each Node which needs this file
        /// 
        /// The Method differentiates between WriteAccess and ReadAccess
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="writeAccess">Mostly the File must be only readable</param>
        /// <returns></returns>
        public IfcStore GetModel(string filePath, bool writeAccess = false)
        {
            if (writeAccess == false)
            {
                return IfcStore.Open(filePath,null ,false);
            }
            else
            {
                return IfcStore.Open(filePath);
            }
        }

        public void CloseModel(IfcStore xModel)
        {
            xModel.Close();
        }

        public IModel GetIModel(string filePath)
        {
            return ModelStorage[filePath].ReferencingModel;
        }
    }
}
