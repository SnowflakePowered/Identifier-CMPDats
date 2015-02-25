using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Snowflake.Plugin;
using System.Reflection;
using System.IO;
using System.Text.RegularExpressions;
using System.ComponentModel.Composition;
using Snowflake.Service;
using Snowflake.Identifier;
using Snowflake.Utility;

namespace Identifier.CMPDats
{
    public sealed class IdentifierCMPDat: BaseIdentifier
    {
        [ImportingConstructor]
        public IdentifierCMPDat([Import("coreInstance")] ICoreService coreInstance)
            : base(Assembly.GetExecutingAssembly(), coreInstance)
        {
            this.InitConfiguration();
        }

   
        private void InitConfiguration()
        {
            using (Stream stream = this.GetResource("config.yml"))
            using (var reader = new StreamReader(stream))
            {
                string file = reader.ReadToEnd();
                this.PluginConfiguration = new YamlConfiguration(Path.Combine(this.PluginDataPath, "config.yml"), file);
                this.PluginConfiguration.LoadConfiguration();
            }
        }

        public override string IdentifyGame(string fileName, string platformId)
        {
            return IdentifyGame(File.OpenRead(fileName), platformId);
        }
        public override string IdentifyGame(FileStream file, string platformId)
        {
            string crc32 = Snowflake.Utility.FileHash.GetCRC32(file);
            file.Close();
            List<object> datFiles = this.PluginConfiguration.Configuration["dats"][platformId];

            var match = datFiles
                .Select(datFile => File.ReadAllText(Path.Combine(this.PluginDataPath, "dats", datFile.ToString())))
                .Select(datFile =>
                        Regex.Match(datFile, String.Format(@"(?<=rom \( name "").*?(?="" size \d+ crc {0})", crc32),
                            RegexOptions.IgnoreCase))
                            .First(gameMatch => gameMatch.Success);
     
            string gameName = Regex.Match(match.Value, @"(\[[^]]*\])*([\w\s]+)").Groups[2].Value;
            
            return gameName;
        }
    }
}
