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
        public Dictionary<Guid, XbimModel> modelStorage;

        private DataController()
        {
            modelStorage = new Dictionary<Guid, XbimModel>();
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
        /// <param name="id"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool AddModel(Guid id, XbimModel model)
        {
            modelStorage.Add(id, model);
            return true;
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="id"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool RemoveModel(Guid id, XbimModel model)
        {
            return true;
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public XbimModel GetModel(Guid id)
        {
            // modelStorage[id].Open(path + "temp_reader" + number + ".xbim")
            // return modelStorage[id];
            return null;
        }
    }
}
