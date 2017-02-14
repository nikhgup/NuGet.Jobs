﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Gallery.CredentialExpiration {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class Strings {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Strings() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Gallery.CredentialExpiration.Strings", typeof(Strings).Assembly);
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
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0} - has expired..
        /// </summary>
        public static string ApiKeyExpired {
            get {
                return ResourceManager.GetString("ApiKeyExpired", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0} - expires in {1} day(s)..
        /// </summary>
        public static string ApiKeyExpiring {
            get {
                return ResourceManager.GetString("ApiKeyExpiring", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Hi {0},
        ///
        ///We wanted to inform you that your following API key(s) on {1} have expired.
        ///
        ///{2}
        ///
        ///Visit {3} to generate a new API key(s) so that you can continue pushing packages.
        ///
        ///Best regards,
        ///{1}.
        /// </summary>
        public static string ExpiredEmailBody {
            get {
                return ResourceManager.GetString("ExpiredEmailBody", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to [{0}] Your API key has expired.
        /// </summary>
        public static string ExpiredEmailSubject {
            get {
                return ResourceManager.GetString("ExpiredEmailSubject", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Hi {0},
        ///
        ///We wanted to inform you that your following API key(s) on {1} will expire soon: 
        ///
        ///{2}
        ///
        ///Visit {3} to generate a new API key(s) so that you can continue pushing packages using them.
        ///
        ///Best regards,
        ///{1}.
        /// </summary>
        public static string ExpiringEmailBody {
            get {
                return ResourceManager.GetString("ExpiringEmailBody", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to [{0}] Your API key is about to expire.
        /// </summary>
        public static string ExpiringEmailSubject {
            get {
                return ResourceManager.GetString("ExpiringEmailSubject", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT cr.[Type], cr.[Created], cr.[Expires], cr.[Description], u.[EmailAddress], u.[Username]
        ///FROM [Credentials] AS cr
        ///INNER JOIN Users AS u ON u.[Key] = cr.[UserKey]
        ///WHERE u.[EmailAllowed] = 1
        ///  AND u.[EmailAddress] &lt;&gt; &apos;&apos;
        ///  AND cr.[Expires] &lt;= DATEADD(day,{0},GETUTCDATE())
        ///  AND cr.[Expires] &gt; DATEADD(day,-1,GETUTCDATE())
        ///  AND (cr.[Type] = &apos;apikey.v1&apos; or cr.[Type] = &apos;apikey.v2&apos;)
        ///ORDER BY u.[Username].
        /// </summary>
        public static string GetExpiredCredentialsQuery {
            get {
                return ResourceManager.GetString("GetExpiredCredentialsQuery", resourceCulture);
            }
        }
    }
}
