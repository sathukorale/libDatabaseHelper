﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:2.0.50727.8670
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DatabaseEntityFormGenerator.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "2.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("DatabaseEntityFormGenerator.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to private void Load{0}()
        ///{
        ///	try
        ///	{
        ///		var items = DatabaseManager.Select&lt;{1}&gt;(null);
        ///		cmb{0}.Items.Clear();
        ///		cmb{0}.Items.AddRange(items);
        ///	}
        ///	catch { }
        ///}.
        /// </summary>
        internal static string ComboLoadingCode {
            get {
                return ResourceManager.GetString("ComboLoadingCode", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to using libDatabaseHelper.forms;
        ///using libDatabaseHelper.classes.generic;
        ///using libDatabaseHelper.classes.sqlce;
        ///using libDatabaseHelper.forms.controls;
        ///
        ///namespace NAMESPACE
        ///{
        ///    partial class CLASS_NAME
        ///    {
        ///        /// &lt;summary&gt;
        ///        /// Required designer variable.
        ///        /// &lt;/summary&gt;
        ///        private System.ComponentModel.IContainer components = null;
        ///
        ///        /// &lt;summary&gt;
        ///        /// Clean up any resources being used.
        ///        /// &lt;/summary&gt;
        ///        /// &lt;param name=&quot;disposing&quot;&gt;true [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string DBEntityForm_AutogenCode {
            get {
                return ResourceManager.GetString("DBEntityForm_AutogenCode", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to using System;
        ///using System.Collections.Generic;
        ///using System.ComponentModel;
        ///using System.Data;
        ///using System.Drawing;
        ///using System.Linq;
        ///using System.Text;
        ///using System.Windows.Forms;
        ///
        ///using libDatabaseHelper.forms;
        ///using libDatabaseHelper.classes.generic;
        ///using libDatabaseHelper.classes.sqlce;
        ///using libDatabaseHelper.forms.controls;
        ///
        ///namespace NAMESPACE
        ///{
        ///    public partial class CLASS_NAME : DatabaseEntityForm
        ///    {
        ///        public CLASS_NAME()
        ///        {
        ///            InitializeComponent( [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string DBEntityForm_UserCode {
            get {
                return ResourceManager.GetString("DBEntityForm_UserCode", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;?xml version=&quot;1.0&quot; encoding=&quot;utf-8&quot;?&gt;
        ///&lt;root&gt;
        ///  &lt;!-- 
        ///    Microsoft ResX Schema 
        ///    
        ///    Version 2.0
        ///    
        ///    The primary goals of this format is to allow a simple XML format 
        ///    that is mostly human readable. The generation and parsing of the 
        ///    various data types are done through the TypeConverter classes 
        ///    associated with the data types.
        ///    
        ///    Example:
        ///    
        ///    ... ado.net/XML headers &amp; schema ...
        ///    &lt;resheader name=&quot;resmimetype&quot;&gt;text/microsoft-resx&lt;/resheader&gt;
        ///    &lt;resheader name=&quot;version&quot;&gt;2. [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string RESXContent {
            get {
                return ResourceManager.GetString("RESXContent", resourceCulture);
            }
        }
    }
}
