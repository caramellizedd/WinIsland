using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace WinIsland.PluginSystem
{
    public class PluginInit
    {
        private static Logger logger = MainWindow.logger;
        public static void loadAll(PluginManager plugman)
        {
            logger.log("===Plugins Init Started===");

            if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "Plugins")) return;

            string[] directories = Directory.GetDirectories(AppDomain.CurrentDomain.BaseDirectory + "Plugins");

            foreach (string dir in directories)
            {
                string dll = $"{dir}\\{Path.GetFileName(dir)}.dll";
                try
                {
                    if (!File.Exists(dll)) continue;
                    logger.log($"Attempting to load {Path.GetFileName(dll)}...");
                    Assembly assembly = Assembly.LoadFrom(dll);

                    List<string> namespaces = new List<string>();
                    foreach (var types in assembly.GetTypes())
                    {
                        string ns = types.Namespace;
                        if (!namespaces.Contains(ns))
                        {
                            namespaces.Add(ns);
                        }
                    }

                    Type? type = assembly.GetType($"{namespaces[0]}.PluginMetadata");

                    if (type == null)
                    {
                        logger.log($"{Path.GetFileName(dll)} is not a plugin or has a missing metadata.");
                        continue;
                    }

                    object? instance = Activator.CreateInstance(type);

                    FieldInfo? name = type.GetField("name", BindingFlags.Public | BindingFlags.Static);
                    FieldInfo? version = type.GetField("version", BindingFlags.Public | BindingFlags.Static);
                    FieldInfo? hasPages = type.GetField("hasPages", BindingFlags.Public | BindingFlags.Static);
                    if (name == null || version == null)
                    {
                        logger.log($"{Path.GetFileName(dll)} is not a plugin or has a missing metadata.");
                        continue;
                    }
                    string? nameValue = (string?)name.GetValue(null);
                    string? versionValue = (string?)version.GetValue(null);
                    bool? hasPagesValue = hasPages == null ? false : (bool?)hasPages.GetValue(null);
                    logger.log("Loaded plugin: " + nameValue + " v" + versionValue);

                    if (hasPagesValue == true)
                    {
                        Type? pageType = assembly.GetType($"{namespaces[0]}.PluginPage");
                        if (pageType == null)
                        {
                            logger.log($"{Path.GetFileName(dll)} claims to have pages but PluginPage class is missing.");
                            continue;
                        }
                        Page? externalPage = (Page?)Activator.CreateInstance(pageType);

                        //MainWindow.instance.islandContent.Navigate(externalPage);
                        plugman.registerPage(externalPage);
                    }
                    else
                    {
                        // TODO: do shit if the plugin is code-only.
                    }

                    plugman.register(assembly);

                    //MethodInfo? method = type.GetMethod("YourFunctionName");
                    //object? result = method.Invoke(instance, null);
                }
                catch (Exception ex)
                {
                    logger.log($"Failed to load {dll}: {ex.Message}");
                    logger.log(ex.StackTrace);
                }
            }

            logger.log("===All Plugins Loaded===");
        }
    }
}
