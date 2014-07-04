using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Mini.Data
{
    public class DbConfig
    {
        private DbConfig(string configFileName,string prvKey, string connKey)
        {
            ExeConfigurationFileMap ecfm = new ExeConfigurationFileMap();
            ecfm.ExeConfigFilename = configFileName;
            Configuration conf = ConfigurationManager.OpenMappedExeConfiguration(ecfm, ConfigurationUserLevel.None);
            this.ProviderInvariantName = conf.AppSettings.Settings[prvKey].Value;
            this.ConnectionString = conf.AppSettings.Settings[connKey].Value;

            if (string.IsNullOrEmpty(this.ProviderInvariantName))
                throw new NullReferenceException("config 需要数据库访问支持器名称。");

            if (string.IsNullOrEmpty(this.ConnectionString))
                throw new NullReferenceException("config 需要数据库访问连接字符串。");
        }

        static object lo = new object();
        static DbConfig dbConfig = null;
        public static DbConfig Get(string configFileName = "", string keyProviderName="prv", string keyConnection="conn")
        {
            if (dbConfig == null)
            {
                if (string.IsNullOrEmpty(configFileName))
                    throw new ArgumentNullException("configFileName");

                lock (lo)
                {
                    if (dbConfig == null)
                        dbConfig = new DbConfig(configFileName, keyProviderName, keyConnection);
                }
            }
            return dbConfig;
        }

        public string ProviderInvariantName
        {
            get;
            private set;
        }

        public string ConnectionString
        {
            get;
            private set;
        }             
    }
}
