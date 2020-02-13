using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace GooseLua {
    public static class ComputerInfo {
        private static string SessionID = "";

        public static string GetInfo() {
            StringBuilder info = new StringBuilder();
            info.AppendLine(GetComputer());
            info.AppendLine(GetOS());
            info.AppendLine(GetCPU());
            string inf = info.ToString().Trim();
            if (string.IsNullOrEmpty(inf)) inf = "N/A";
            return inf;
        }

        public static string GetSessionID() {
            if (string.IsNullOrEmpty(SessionID)) SessionID = GenerateSessionID();
            return SessionID;
        }

        private static string GenerateSessionID(int length = 16, string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890-_") {
            using (RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider()) {
                byte[] data = new byte[length];
                byte[] buffer = null;
                int maxRandom = byte.MaxValue - ((byte.MaxValue + 1) % chars.Length);
                crypto.GetBytes(data);
                char[] result = new char[length];
                for (int i = 0; i < length; i++) {
                    byte value = data[i];
                    while (value > maxRandom) {
                        if (buffer == null) {
                            buffer = new byte[1];
                        }
                        crypto.GetBytes(buffer);
                        value = buffer[0];
                    }
                    result[i] = chars[value % chars.Length];
                }
                return new string(result);
            }
        }

        public static string GetComputer() {
            StringBuilder info = new StringBuilder();
            info.AppendLine("User Name: " + Environment.UserName);
            info.AppendLine("Domain Name: " + Environment.UserDomainName);
            string inf = info.ToString().Trim();
            if (string.IsNullOrEmpty(inf)) inf = "N/A";
            return inf;
        }

        public static string GetOS() {
            StringBuilder info = new StringBuilder();
            ManagementObjectSearcher mos = new ManagementObjectSearcher("select * from Win32_OperatingSystem");
            foreach (ManagementObject managementObject in mos.Get()) {
                if (managementObject["Caption"] != null) {
                    info.AppendLine("OS:  " + managementObject["Caption"].ToString());
                }
                if (managementObject["OSArchitecture"] != null) {
                    info.AppendLine("OS Architecture:  " + managementObject["OSArchitecture"].ToString());
                }
                if (managementObject["CSDVersion"] != null) {
                    info.AppendLine("Service Pack:  " + managementObject["CSDVersion"].ToString());
                }
            }
            string inf = info.ToString().Trim();
            if (string.IsNullOrEmpty(inf)) inf = "N/A";
            return inf;
        }

        public static string GetCPU() {
            RegistryKey processor_name = Registry.LocalMachine.OpenSubKey(@"Hardware\Description\System\CentralProcessor\0", RegistryKeyPermissionCheck.ReadSubTree);   //This registry entry contains entry for processor info.

            if (processor_name != null) {
                if (processor_name.GetValue("ProcessorNameString") != null) {
                    return processor_name.GetValue("ProcessorNameString").ToString();
                }
            }

            return "N/A";
        }
    }
}
