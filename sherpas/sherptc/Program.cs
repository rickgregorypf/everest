﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MohawkCollege.Util.Console.Parameters;
using MARC.Everest.Sherpas.Templating.Loader;
using System.Globalization;
using MARC.Everest.Sherpas.Templating.Format;
using System.IO;
using System.Reflection;
using MARC.Everest.Sherpas.Templating.Binder;
using System.Xml.Serialization;
using System.CodeDom;

namespace sherptc
{
    public class Program
    {

        /// <summary>
        /// The main entry point to the program
        /// </summary>
        public static void Main(String[] args)
        {

            // Parser for parameters
            ParameterParser<ConsoleParameters> parser = new ParameterParser<ConsoleParameters>();

            try
            {

                Console.WriteLine("SHERPAS CDA Template Compiler for MARC-HI Everest Framework");
                Console.WriteLine("Copyright (C) 2014, Mohawk College of Applied Arts and Technology");

                // Parse parameters
                var parameters = parser.Parse(args);

                // First, load the specified loader
                XsltBasedLoader loader = new XsltBasedLoader();
                loader.Locale = parameters.Locale ?? CultureInfo.CurrentCulture.Name;

                if (String.IsNullOrEmpty(parameters.EverestAssembly) || !File.Exists(parameters.EverestAssembly))
                    throw new FileNotFoundException(String.Format("Couldn't find the specified Everest assembly file '{0}'...", parameters.EverestAssembly));
                var asm = Assembly.LoadFile(parameters.EverestAssembly);

                // Now load
                Console.Out.WriteLineIf(parameters.Verbose, "Loading template files into template project...");
                TemplateProjectDefinition compileTemplate = new TemplateProjectDefinition();
                foreach (var s in parameters.Template)
                {
                    Console.Out.WriteLineIf(parameters.Verbose, "\t{0}", Path.GetFileNameWithoutExtension(s));
                    compileTemplate.Merge(loader.LoadTemplate(s));
                }

                // Now compile or normalize the template ... I.e. bind them
                Console.Out.WriteLineIf(parameters.Verbose, "Binding template project to assembly '{0}'...", asm.GetName().Name);
                for(int i = 0; i < compileTemplate.Templates.Count; i++) 
                {
                    var tpl = compileTemplate.Templates[i];
                    BindingContext ctx = new BindingContext(asm, tpl, compileTemplate);
                    var binder = ctx.GetBinder();
                    if (binder == null)
                        Console.Error.WriteLineIf(parameters.Verbose, "Cannot find a valid Binder for template of type '{0}'", tpl.GetType().Name);
                    else
                        binder.Bind(ctx);
                }

                // Save bound template?
                if (!String.IsNullOrEmpty(parameters.SaveTpl))
                    compileTemplate.Save(parameters.SaveTpl);
            }
            catch (Exception e)
            {

#if DEBUG
                Console.WriteLine(e.ToString());
#else
                Console.WriteLine(e.Message);

#endif
            }
            finally
            {
#if DEBUG
                Console.ReadKey();
#endif
            }


        }

    }
}
