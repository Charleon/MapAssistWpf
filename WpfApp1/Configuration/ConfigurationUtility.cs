
using System;
using System.Configuration;
using System.Globalization;

namespace MapAssist.Settings
{
    public class ConfigurationReadException : Exception
    {
        public ConfigurationReadException(string parameterName, string type ,string optionalInfo = "") : base($"Error in parsing configuration! {parameterName} could not be parsed as {type}. {optionalInfo}")
        {
        }
    }

    /// <summary>
    /// Reads the configuration in a uniform way
    /// </summary>
    public static class ConfigurationReader
    {
        public static string ReadString(string configurationParameter)
        {
            string result = "";

            try
            {
                result = ConfigurationManager.AppSettings[configurationParameter];
            } 
            catch(Exception)
            {
                throw new ConfigurationReadException(configurationParameter, result.GetType().ToString());
            }

            return result;
        }
        public static bool ReadBoolean(string configurationParameter)
        {
            bool result = false;
            
            try 
            { 
                result = Convert.ToBoolean(ConfigurationManager.AppSettings[configurationParameter]);
            }
            catch(Exception)
            {
                throw new ConfigurationReadException(configurationParameter, result.GetType().ToString());
            }

            return result;
        }

        public static short ReadInt16(string configurationParameter)
        {
            short result = 0;

            try
            {
                result = Convert.ToInt16(ConfigurationManager.AppSettings[configurationParameter]);
            }
            catch (Exception)
            {
                throw new ConfigurationReadException(configurationParameter, result.GetType().ToString());
            }

            return result;
        }

        public static int ReadInt32(string configurationParameter, int? optionalBase = null)
        {
            int result = 0;

            try 
            { 
                if(optionalBase == null) 
                { 
                    result = Convert.ToInt32(ConfigurationManager.AppSettings[configurationParameter]);
                } 
                else
                {
                    result = Convert.ToInt32(ConfigurationManager.AppSettings[configurationParameter], optionalBase.Value);
                }
            }
            catch (Exception)
            {
                throw new ConfigurationReadException(configurationParameter, result.GetType().ToString());
            }

            return result;
        }

        public static char ReadChar(string configurationParameter)
        {
            char result = 'a';
            
            try
            {
                result = Convert.ToChar(ConfigurationManager.AppSettings[configurationParameter]);
            }
            catch (Exception)
            {
                throw new ConfigurationReadException(configurationParameter, result.GetType().ToString());
            }

            return result;
        }

        public static float ReadSingle(string configurationParameter)
        {
            float result = 0f;

            try
            {
                result = Convert.ToSingle(ConfigurationManager.AppSettings[configurationParameter], CultureInfo.InvariantCulture);
            }
            catch (Exception)
            {
                throw new ConfigurationReadException(configurationParameter, result.GetType().ToString());
            }

            return result;
        }

        public static double ReadDouble(string configurationParameter)
        {
            double result = 0d;
            
            try
            {
                result = Convert.ToDouble(ConfigurationManager.AppSettings[configurationParameter], CultureInfo.InvariantCulture);
            }
            catch (Exception)
            {
                throw new ConfigurationReadException(configurationParameter, result.GetType().ToString());
            }

            return result;
        }

        public static T ParseEnum<T>(string configurationParameter)
        {
            if (!typeof(T).IsEnum)
            {
                throw new ConfigurationReadException(configurationParameter, typeof(T).ToString());
            }
            
            try
            {
                T value = (T)Enum.Parse(typeof(T), ConfigurationManager.AppSettings[configurationParameter], true);
                return value;
            }
            catch (Exception)
            {
                var enumValues = Enum.GetValues(typeof(T));
                string enumValuesString = "";
                foreach(var value in  enumValues)
                {
                    enumValuesString += $"{value} ";
                }
                throw new ConfigurationReadException(configurationParameter, typeof(T).ToString(), $"Valid values are: {enumValuesString}");
            }

        }
    }
}
