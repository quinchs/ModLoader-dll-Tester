using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using InternalModding.Assemblies;
using InternalModding.Misc;
using UnityEngine;

namespace InternalModding.Mods
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class RequireToValidateAttribute : Attribute
    {
    }
    public class ResourceReference : Element
    {
        // Token: 0x060025CB RID: 9675 RVA: 0x000D26B4 File Offset: 0x000D08B4
        protected override bool Validate(string elementName)
        {
            return base.Validate(elementName) && (!string.IsNullOrEmpty(this.Name) || base.InvalidData(elementName, "name attribute must not be empty!"));
        }

        // Token: 0x040022F7 RID: 8951
        [XmlAttribute("name")]
        public string Name;
    }
    [Serializable]
    public class Element
    {
        // Token: 0x060025EB RID: 9707 RVA: 0x000D2DC8 File Offset: 0x000D0FC8
        public Element()
        {
            this.AttributesUsed = string.Empty;
            this.ElementsUsed = string.Empty;
            this.FileName = "unknown";
        }

        // Token: 0x170003AB RID: 939
        // (get) Token: 0x060025EC RID: 9708 RVA: 0x000D2E30 File Offset: 0x000D1030
        // (set) Token: 0x060025ED RID: 9709 RVA: 0x000D2E38 File Offset: 0x000D1038
        [XmlAttribute("lineNumber")]
        public int LineNumber { get;  set; }

        // Token: 0x170003AC RID: 940
        // (get) Token: 0x060025EE RID: 9710 RVA: 0x000D2E44 File Offset: 0x000D1044
        // (set) Token: 0x060025EF RID: 9711 RVA: 0x000D2E4C File Offset: 0x000D104C
        [XmlAttribute("linePosition")]
        public int LinePosition { get;  set; }

        // Token: 0x170003AD RID: 941
        // (get) Token: 0x060025F0 RID: 9712 RVA: 0x000D2E58 File Offset: 0x000D1058
        // (set) Token: 0x060025F1 RID: 9713 RVA: 0x000D2E60 File Offset: 0x000D1060
        [XmlAttribute("attributesUsed")]
        [DefaultValue("")]
        public string AttributesUsed { get;  set; }

        // Token: 0x170003AE RID: 942
        // (get) Token: 0x060025F2 RID: 9714 RVA: 0x000D2E6C File Offset: 0x000D106C
        // (set) Token: 0x060025F3 RID: 9715 RVA: 0x000D2E74 File Offset: 0x000D1074
        [XmlAttribute("elementsUsed")]
        [DefaultValue("")]
        public string ElementsUsed { get;  set; }

        // Token: 0x170003AF RID: 943
        // (get) Token: 0x060025F4 RID: 9716 RVA: 0x000D2E80 File Offset: 0x000D1080
        // (set) Token: 0x060025F5 RID: 9717 RVA: 0x000D2E88 File Offset: 0x000D1088
        [XmlAttribute("fileName")]
        [DefaultValue("unknown")]
        public string FileName { get;  set; }

        // Token: 0x060025F6 RID: 9718 RVA: 0x000D2E94 File Offset: 0x000D1094
        internal bool InvokeValidate()
        {
            return this.Validate();
        }

        // Token: 0x060025F7 RID: 9719 RVA: 0x000D2E9C File Offset: 0x000D109C
        internal bool InvokeValidate(string elementName)
        {
            return this.Validate(elementName);
        }

        // Token: 0x060025F8 RID: 9720 RVA: 0x000D2EA8 File Offset: 0x000D10A8
        protected virtual bool Validate()
        {
            return this.Validate(base.GetType().Name);
        }

        // Token: 0x060025F9 RID: 9721 RVA: 0x000D2EBC File Offset: 0x000D10BC
        protected virtual bool Validate(string elementName)
        {
            return true;
        }

        // Token: 0x060025FA RID: 9722 RVA: 0x000D2EC8 File Offset: 0x000D10C8
        protected bool MissingElement(string elemName, string missing)
        {
            return true;
        }

        // Token: 0x060025FB RID: 9723 RVA: 0x000D2ED4 File Offset: 0x000D10D4
        protected bool MissingAttribute(string elemName, string missing)
        {
            return true;
        }

        // Token: 0x060025FC RID: 9724 RVA: 0x000D2EE0 File Offset: 0x000D10E0
        protected bool InvalidData(string elemName, string error)
        {
            return true;
        }

        // Token: 0x060025FD RID: 9725 RVA: 0x000D2EEC File Offset: 0x000D10EC
        protected void Warn(string elemName, string warning)
        {
            Assemblies.MLog.Warn($"[{elemName}] {warning}");
        }

        // Token: 0x04002316 RID: 8982
        internal string[] SpecialAttributeNames = new string[]
        {
            "lineNumber",
            "linePosition",
            "attributesUsed",
            "elementsUsed",
            "fileName"
        };
    }
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class CanBeEmptyAttribute : Attribute
    {
    }
    public static class ModXmlLoader
    {
        // Token: 0x06000E4E RID: 3662 RVA: 0x00060B84 File Offset: 0x0005ED84
        public static T Deserialize<T>(string path, bool validate) where T : Element
        {
            T result;
            try
            {
                using (FileStream fileStream = new FileStream(path, FileMode.Open))
                {
                    using (StreamReader streamReader = new StreamReader(fileStream))
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof(T));
                        XDocument xDoc = XDocument.Load(streamReader, LoadOptions.SetLineInfo);
                        string name = new FileInfo(path).Name;
                        result = ModXmlLoader.Deserialize<T>(xDoc, serializer, validate, name, 0);
                    }
                }
            }
            catch (Exception ex)
            {
                Assemblies.MLog.Error("Error loading mod XML file: " + new FileInfo(path).Name);
                Assemblies.MLog.Error(ex.ToString());
                result = (T)((object)null);
            }
            return result;
        }

        // Token: 0x06000E4F RID: 3663 RVA: 0x00060C88 File Offset: 0x0005EE88
        public static T Deserialize<T>(string content, bool validate, string fileName, int lineOffset, Type serializerType = null) where T : Element
        {
            T result;
            try
            {
                XmlSerializer serializer;
                if (serializerType == null)
                {
                    serializer = new XmlSerializer(typeof(T));
                }
                else
                {
                    serializer = new XmlSerializer(serializerType);
                }
                XDocument xDoc = XDocument.Load(new StringReader(content), LoadOptions.SetLineInfo);
                result = ModXmlLoader.Deserialize<T>(xDoc, serializer, validate, fileName, lineOffset);
            }
            catch (Exception ex)
            {
                Assemblies.MLog.Error("Error loading mod XML file: " + fileName);
                Assemblies.MLog.Error(ex.ToString());
                result = (T)((object)null);
            }
            return result;
        }

        // Token: 0x06000E50 RID: 3664 RVA: 0x00060D28 File Offset: 0x0005EF28
        private static T Deserialize<T>(XDocument xDoc, XmlSerializer serializer, bool validate, string fileName, int lineOffset) where T : Element
        {
            ModXmlLoader.PreProcess(xDoc.Root, fileName, lineOffset);
            serializer.UnknownNode += delegate (object sender, XmlNodeEventArgs args)
            {
                if (args.Text == ">")
                {
                    Assemblies.MLog.WarnFormat("In {0}: There appears to be an extraneous '>' character somewhere in the file.This can cause deserialization of some elements to silently fail!", new object[]
                    {
                        fileName
                    });
                }
            };
            T t = (T)((object)serializer.Deserialize(xDoc.CreateReader()));
            if (t == null)
            {
                return (T)((object)null);
            }
            if (validate && !t.InvokeValidate())
            {
                Assemblies.MLog.Error("Error loading " + fileName);
                return (T)((object)null);
            }
            return t;
        }

        // Token: 0x06000E51 RID: 3665 RVA: 0x00060DC0 File Offset: 0x0005EFC0
        private static void PreProcess(XElement element, string fileName, int lineOffset = 0)
        {
            string[] array = (from a in element.Attributes()
                              select a.Name.LocalName).ToArray<string>();
            string[] array2 = (from e in element.Elements()
                               select e.Name.LocalName).ToArray<string>();
            if (!string.IsNullOrEmpty(element.Value))
            {
                if (element.Value == "True")
                {
                    element.Value = "true";
                }
                else if (element.Value == "False")
                {
                    element.Value = "false";
                }
                string text = element.Value.TrimStart(new char[]
                {
                    '\n',
                    '\r'
                }).TrimEnd(new char[0]);
                if (text != element.Value)
                {
                    element.Value = text;
                }
            }
            foreach (XAttribute xattribute in element.Attributes())
            {
                if (xattribute.Value == "True")
                {
                    xattribute.Value = "true";
                }
                else if (xattribute.Value == "False")
                {
                    element.Value = "false";
                }
            }
            foreach (XElement element2 in element.Elements())
            {
                ModXmlLoader.PreProcess(element2, fileName, lineOffset);
            }
            element.SetAttributeValue("lineNumber", ((IXmlLineInfo)element).LineNumber + lineOffset);
            element.SetAttributeValue("linePosition", ((IXmlLineInfo)element).LinePosition);
            element.SetAttributeValue("fileName", fileName);
            if (array2.Length > 0)
            {
                element.SetAttributeValue("elementsUsed", string.Join("|", array2));
            }
            if (array.Length > 0)
            {
                element.SetAttributeValue("attributesUsed", string.Join("|", array));
            }
        }
    }
    // Token: 0x0200027E RID: 638
    [XmlRoot("Mod")]
    public class ModInfo : Element
    {
        // Token: 0x06000FD5 RID: 4053 RVA: 0x000684F8 File Offset: 0x000666F8
        public ModInfo()
        {
            this.Assemblies = new List<ModInfo.AssemblyInfo>();
            this.Blocks = new List<ModInfo.BlockInfo>();
            this.Entities = new List<ModInfo.EntityInfo>();
            this.ResourceChoices = new ModInfo.ResourcesChoices();
            this.Triggers = new List<ModInfo.TriggerInfo>();
            this.Events = new List<ModInfo.EventInfo>();
            this.Keys = new List<ModInfo.KeyInfo>();
            this.Id = Guid.Empty;
            this.LoadOrder = 0;
            this.FromWorkshop = false;
            this.WorkshopId = 0UL;
        }

        // Token: 0x1700014C RID: 332
        // (get) Token: 0x06000FD6 RID: 4054 RVA: 0x0006857C File Offset: 0x0006677C
        // (set) Token: 0x06000FD7 RID: 4055 RVA: 0x00068584 File Offset: 0x00066784
        [XmlIgnore]
        public Guid Id { get; set; }

        // Token: 0x1700014D RID: 333
        // (get) Token: 0x06000FD8 RID: 4056 RVA: 0x00068590 File Offset: 0x00066790
        // (set) Token: 0x06000FD9 RID: 4057 RVA: 0x00068598 File Offset: 0x00066798
        [XmlElement("ID")]
        [DefaultValue("")]
        public string IdFromFile { get;  set; }

        // Token: 0x1700014E RID: 334
        // (get) Token: 0x06000FDA RID: 4058 RVA: 0x000685A4 File Offset: 0x000667A4
        // (set) Token: 0x06000FDB RID: 4059 RVA: 0x000685AC File Offset: 0x000667AC
        [XmlIgnore]
        public bool FromWorkshop { get; set; }

        // Token: 0x1700014F RID: 335
        // (get) Token: 0x06000FDC RID: 4060 RVA: 0x000685B8 File Offset: 0x000667B8
        // (set) Token: 0x06000FDD RID: 4061 RVA: 0x000685C0 File Offset: 0x000667C0
        [XmlIgnore]
        public ulong WorkshopId { get; set; }

        // Token: 0x17000150 RID: 336
        // (get) Token: 0x06000FDE RID: 4062 RVA: 0x000685CC File Offset: 0x000667CC
        // (set) Token: 0x06000FDF RID: 4063 RVA: 0x000685D4 File Offset: 0x000667D4
        [XmlElement]
        public string Name { get; set; }

        // Token: 0x17000151 RID: 337
        // (get) Token: 0x06000FE0 RID: 4064 RVA: 0x000685E0 File Offset: 0x000667E0
        // (set) Token: 0x06000FE1 RID: 4065 RVA: 0x000685E8 File Offset: 0x000667E8
        [XmlElement]
        public string Author { get; set; }

        // Token: 0x17000152 RID: 338
        // (get) Token: 0x06000FE2 RID: 4066 RVA: 0x000685F4 File Offset: 0x000667F4
        // (set) Token: 0x06000FE3 RID: 4067 RVA: 0x000685FC File Offset: 0x000667FC
        [XmlElement]
        public string Description { get; set; }

        // Token: 0x17000153 RID: 339
        // (get) Token: 0x06000FE4 RID: 4068 RVA: 0x00068608 File Offset: 0x00066808
        // (set) Token: 0x06000FE5 RID: 4069 RVA: 0x00068610 File Offset: 0x00066810
        [XmlElement("Icon")]
        [RequireToValidate]
        [DefaultValue(null)]
        public ResourceReference IconReference { get; set; }

        // Token: 0x17000154 RID: 340
        // (get) Token: 0x06000FE6 RID: 4070 RVA: 0x0006861C File Offset: 0x0006681C
        // (set) Token: 0x06000FE7 RID: 4071 RVA: 0x00068624 File Offset: 0x00066824
        
        // Token: 0x17000155 RID: 341
        // (get) Token: 0x06000FE8 RID: 4072 RVA: 0x00068630 File Offset: 0x00066830
        // (set) Token: 0x06000FE9 RID: 4073 RVA: 0x00068638 File Offset: 0x00066838
        [XmlElement("WorkshopThumbnail")]
        [RequireToValidate]
        [DefaultValue(null)]
        public ResourceReference WorkshopThumbnailReference { get; set; }

        // Token: 0x17000156 RID: 342
        // (get) Token: 0x06000FEA RID: 4074 RVA: 0x00068644 File Offset: 0x00066844
        // (set) Token: 0x06000FEB RID: 4075 RVA: 0x0006864C File Offset: 0x0006684C
        
        // Token: 0x17000157 RID: 343
        // (get) Token: 0x06000FEC RID: 4076 RVA: 0x00068658 File Offset: 0x00066858
        // (set) Token: 0x06000FED RID: 4077 RVA: 0x00068660 File Offset: 0x00066860
        
        // Token: 0x17000158 RID: 344
        // (get) Token: 0x06000FEE RID: 4078 RVA: 0x0006866C File Offset: 0x0006686C
        // (set) Token: 0x06000FEF RID: 4079 RVA: 0x00068674 File Offset: 0x00066874
        [XmlElement("Debug")]
        [DefaultValue(false)]
        public bool DebugEnabled { get; set; }

        // Token: 0x17000159 RID: 345
        // (get) Token: 0x06000FF0 RID: 4080 RVA: 0x00068680 File Offset: 0x00066880
        // (set) Token: 0x06000FF1 RID: 4081 RVA: 0x00068688 File Offset: 0x00066888
        [XmlElement]
        public bool MultiplayerCompatible { get; set; }

        // Token: 0x1700015A RID: 346
        // (get) Token: 0x06000FF2 RID: 4082 RVA: 0x00068694 File Offset: 0x00066894
        // (set) Token: 0x06000FF3 RID: 4083 RVA: 0x0006869C File Offset: 0x0006689C
        [XmlElement("LoadInTitleScreen")]
        [DefaultValue(null)]
        [RequireToValidate]
        public Element LoadInTitleScreenElement { get; set; }

        // Token: 0x1700015B RID: 347
        // (get) Token: 0x06000FF4 RID: 4084 RVA: 0x000686A8 File Offset: 0x000668A8
        [XmlIgnore]
        public bool LoadInTitleScreen
        {
            get
            {
                return this.LoadInTitleScreenElement != null;
            }
        }

        // Token: 0x1700015C RID: 348
        // (get) Token: 0x06000FF5 RID: 4085 RVA: 0x000686B8 File Offset: 0x000668B8
        // (set) Token: 0x06000FF6 RID: 4086 RVA: 0x000686C0 File Offset: 0x000668C0
        [XmlElement("LoadOrder")]
        [DefaultValue(0)]
        public int LoadOrder { get; set; }

        // Token: 0x1700015D RID: 349
        // (get) Token: 0x06000FF7 RID: 4087 RVA: 0x000686CC File Offset: 0x000668CC
        // (set) Token: 0x06000FF8 RID: 4088 RVA: 0x000686D4 File Offset: 0x000668D4
        [XmlIgnore]
        public string Directory { get; set; }

        // Token: 0x1700015E RID: 350
        // (get) Token: 0x06000FF9 RID: 4089 RVA: 0x000686E0 File Offset: 0x000668E0
        // (set) Token: 0x06000FFA RID: 4090 RVA: 0x000686E8 File Offset: 0x000668E8
        [XmlArray]
        [CanBeEmpty]
        [RequireToValidate]
        [XmlArrayItem("Assembly", typeof(ModInfo.AssemblyInfo))]
        [XmlArrayItem("ScriptAssembly", typeof(ModInfo.ScriptAssemblyInfo))]
        public List<ModInfo.AssemblyInfo> Assemblies { get; set; }

        // Token: 0x1700015F RID: 351
        // (get) Token: 0x06000FFB RID: 4091 RVA: 0x000686F4 File Offset: 0x000668F4
        // (set) Token: 0x06000FFC RID: 4092 RVA: 0x000686FC File Offset: 0x000668FC
        [XmlArray]
        [CanBeEmpty]
        [RequireToValidate]
        [XmlArrayItem("Block", typeof(ModInfo.BlockInfo))]
        public List<ModInfo.BlockInfo> Blocks { get; set; }

        // Token: 0x17000160 RID: 352
        // (get) Token: 0x06000FFD RID: 4093 RVA: 0x00068708 File Offset: 0x00066908
        // (set) Token: 0x06000FFE RID: 4094 RVA: 0x00068710 File Offset: 0x00066910
        [XmlArray]
        [CanBeEmpty]
        [RequireToValidate]
        [XmlArrayItem("Entity", typeof(ModInfo.EntityInfo))]
        public List<ModInfo.EntityInfo> Entities { get; set; }

        // Token: 0x17000161 RID: 353
        // (get) Token: 0x06000FFF RID: 4095 RVA: 0x0006871C File Offset: 0x0006691C
        // (set) Token: 0x06001000 RID: 4096 RVA: 0x00068724 File Offset: 0x00066924
        [XmlElement("Resources")]
        [DefaultValue(null)]
        [RequireToValidate]
        public ModInfo.ResourcesChoices ResourceChoices { get; set; }

        // Token: 0x17000162 RID: 354
        // (get) Token: 0x06001001 RID: 4097 RVA: 0x00068730 File Offset: 0x00066930
        [XmlIgnore]
        public List<ModInfo.ResourceInfo> Resources
        {
            get
            {
                return this.ResourceChoices;
            }
        }

        // Token: 0x17000163 RID: 355
        // (get) Token: 0x06001002 RID: 4098 RVA: 0x00068740 File Offset: 0x00066940
        // (set) Token: 0x06001003 RID: 4099 RVA: 0x00068748 File Offset: 0x00066948
        [XmlArray]
        [CanBeEmpty]
        [RequireToValidate]
        [XmlArrayItem("Trigger", typeof(ModInfo.TriggerInfo))]
        public List<ModInfo.TriggerInfo> Triggers { get;  set; }

        // Token: 0x17000164 RID: 356
        // (get) Token: 0x06001004 RID: 4100 RVA: 0x00068754 File Offset: 0x00066954
        // (set) Token: 0x06001005 RID: 4101 RVA: 0x0006875C File Offset: 0x0006695C
        [XmlArray]
        [CanBeEmpty]
        [XmlArrayItem("Event", typeof(ModInfo.EventInfo))]
        public List<ModInfo.EventInfo> Events { get;  set; }

        // Token: 0x17000165 RID: 357
        // (get) Token: 0x06001006 RID: 4102 RVA: 0x00068768 File Offset: 0x00066968
        // (set) Token: 0x06001007 RID: 4103 RVA: 0x00068770 File Offset: 0x00066970
        [XmlArray]
        [CanBeEmpty]
        [RequireToValidate]
        [XmlArrayItem("Key", typeof(ModInfo.KeyInfo))]
        public List<ModInfo.KeyInfo> Keys { get;  set; }

        // Token: 0x06001008 RID: 4104 RVA: 0x0006877C File Offset: 0x0006697C
        protected override bool Validate(string elemName)
        {
            if (!base.Validate(elemName))
            {
                return false;
            }
            if (!this.MultiplayerCompatible && this.LoadInTitleScreen)
            {
                return base.InvalidData("LoadInTitleScreen", "Cannot specify LoadInTitleScreen in a singleplayer-only mod!");
            }
            return true;
        }

        // Token: 0x06001009 RID: 4105 RVA: 0x000687F0 File Offset: 0x000669F0
        public bool Equals(object obj)
        {
            ModInfo modInfo = obj as ModInfo;
            return modInfo != null && modInfo.Id == this.Id;
        }

        // Token: 0x0600100A RID: 4106 RVA: 0x00068820 File Offset: 0x00066A20
        public int GetHashCode()
        {
            return this.Id.GetHashCode();
        }

        // Token: 0x0600100B RID: 4107 RVA: 0x0006883C File Offset: 0x00066A3C
        public static ModInfo LoadFromFile(string path, bool debugInfo)
        {
            ModInfo modInfo = ModXmlLoader.Deserialize<ModInfo>(path, false);
            if (modInfo == null)
            {
                InternalModding.Assemblies.MLog.Error("Could not load " + new FileInfo(path));
                return null;
            }
            modInfo.Directory = new FileInfo(path).Directory.FullName;
            for (int i = 0; i < modInfo.Events.Count; i++)
            {
                ModInfo.EventInfo eventInfo = modInfo.Events[i];
                if (!eventInfo.Validate("Event"))
                {
                    InternalModding.Assemblies.MLog.Error("Not loading the mod manifest.");
                    return null;
                }
                if (!string.IsNullOrEmpty(eventInfo.Path))
                {
                    modInfo.Events[i] = ModXmlLoader.Deserialize<ModInfo.EventInfo>(ModPaths.GetFilePath(modInfo, eventInfo.Path, false), false);
                    if (modInfo.Events[i] == null)
                    {
                        InternalModding.Assemblies.MLog.Error("Not loading the mod manifest.");
                        return null;
                    }
                }
            }
            foreach (ModInfo.EventInfo eventInfo2 in modInfo.Events)
            {
                //eventInfo2.CreateProperties();
            }
            if (modInfo.IdFromFileSpecified)
            {
                modInfo.Id = new Guid(modInfo.IdFromFile);
            }
            else
            {
                modInfo.Id = Guid.NewGuid();
                InternalModding.Assemblies.MLog.Info(string.Concat(new object[]
                {
                    "Generated an ID for ",
                    modInfo.Name,
                    " (",
                    modInfo.Id,
                    "), writing it to the file."
                }));
                ModInfo.WriteIdElement(modInfo);
            }
            if (!modInfo.Validate())
            {
                InternalModding.Assemblies.MLog.Error("There was an error loading the mod manifest: " + path);
                return null;
            }
            modInfo.Description = modInfo.Description.Trim();
            foreach (ModInfo.AssemblyInfo assemblyInfo in modInfo.Assemblies)
            {
                assemblyInfo.Path = ModPaths.GetFilePath(modInfo, assemblyInfo.Path, false);
            }
            foreach (ModInfo.BlockInfo blockInfo in modInfo.Blocks)
            {
                blockInfo.Path = ModPaths.GetFilePath(modInfo, blockInfo.Path, false);
            }
            foreach (ModInfo.EntityInfo entityInfo in modInfo.Entities)
            {
                entityInfo.Path = ModPaths.GetFilePath(modInfo, entityInfo.Path, false);
            }
            if (modInfo.ResourceChoices.Resources == null)
            {
                modInfo.ResourceChoices.Resources = new ModInfo.ResourceInfo[0];
                //modInfo.ResourceChoices.ResourceTypes = new ModResource.ResourceType[0];
            }
            for (int j = 0; j < modInfo.ResourceChoices.Resources.Length; j++)
            {
                ModInfo.ResourceInfo resourceInfo = modInfo.ResourceChoices.Resources[j];
                resourceInfo.Path = ModPaths.GetFilePath(modInfo, resourceInfo.Path, true);
                //resourceInfo.Type = modInfo.ResourceChoices.ResourceTypes[j];
            }
            return modInfo;
        }

        // Token: 0x0600100C RID: 4108 RVA: 0x00068BD0 File Offset: 0x00066DD0
        private static void WriteIdElement(ModInfo info)
        {
            string path = Path.Combine(info.Directory, "Mod.xml");
            string text = File.ReadAllText(path);
            text = text.Replace("</Mod>", string.Empty);
            text += ModInfo.GetNewIdText(info.Id);
            text += "</Mod>";
            File.WriteAllText(path, text, Encoding.UTF8);
        }

        // Token: 0x0600100D RID: 4109 RVA: 0x00068C30 File Offset: 0x00066E30
        public static string GetNewIdText(Guid id)
        {
            return "\r\n<!-- This value is automatically generated. Do not change it or you may break machine&level save files. -->\r\n<ID>" + id + "</ID>\r\n";
        }

        // Token: 0x04001068 RID: 4200
        [XmlIgnore]
        public bool IdFromFileSpecified;

        // Token: 0x0200027F RID: 639
        [Serializable]
        public class AssemblyInfo : Element
        {
            // Token: 0x17000166 RID: 358
            // (get) Token: 0x06001010 RID: 4112 RVA: 0x00068C60 File Offset: 0x00066E60
            // (set) Token: 0x06001011 RID: 4113 RVA: 0x00068C68 File Offset: 0x00066E68
            [XmlIgnore]
            public ModContainer Mod { get; set; }

            // Token: 0x17000167 RID: 359
            // (get) Token: 0x06001012 RID: 4114 RVA: 0x00068C74 File Offset: 0x00066E74
            // (set) Token: 0x06001013 RID: 4115 RVA: 0x00068C7C File Offset: 0x00066E7C
            [XmlAttribute("path")]
            public string Path { get; set; }

            // Token: 0x06001014 RID: 4116 RVA: 0x00068C88 File Offset: 0x00066E88
            public virtual void Resolve()
            {
            }
        }

        // Token: 0x02000280 RID: 640
        [Serializable]
        public class ScriptAssemblyInfo : ModInfo.AssemblyInfo
        {
            // Token: 0x06001016 RID: 4118 RVA: 0x00068C94 File Offset: 0x00066E94
            public override void Resolve()
            {
                base.Path = AssemblyCompiler.ResolveScriptAssembly(base.Path, base.Mod);
            }
        }

        // Token: 0x02000281 RID: 641
        [Serializable]
        public class BlockInfo : Element
        {
            // Token: 0x17000168 RID: 360
            // (get) Token: 0x06001018 RID: 4120 RVA: 0x00068CB8 File Offset: 0x00066EB8
            // (set) Token: 0x06001019 RID: 4121 RVA: 0x00068CC0 File Offset: 0x00066EC0
            [XmlIgnore]
            public ModContainer Mod { get; set; }

            // Token: 0x17000169 RID: 361
            // (get) Token: 0x0600101A RID: 4122 RVA: 0x00068CCC File Offset: 0x00066ECC
            // (set) Token: 0x0600101B RID: 4123 RVA: 0x00068CD4 File Offset: 0x00066ED4
            [XmlAttribute("path")]
            public string Path { get; set; }
        }

        // Token: 0x02000282 RID: 642
        [Serializable]
        public class EntityInfo : Element
        {
            // Token: 0x1700016A RID: 362
            // (get) Token: 0x0600101D RID: 4125 RVA: 0x00068CE8 File Offset: 0x00066EE8
            // (set) Token: 0x0600101E RID: 4126 RVA: 0x00068CF0 File Offset: 0x00066EF0
            [XmlIgnore]
            public ModContainer Mod { get; set; }

            // Token: 0x1700016B RID: 363
            // (get) Token: 0x0600101F RID: 4127 RVA: 0x00068CFC File Offset: 0x00066EFC
            // (set) Token: 0x06001020 RID: 4128 RVA: 0x00068D04 File Offset: 0x00066F04
            [XmlAttribute("path")]
            public string Path { get; set; }
        }

        // Token: 0x02000283 RID: 643
        [Serializable]
        public class ResourceInfo : Element
        {
            // Token: 0x06001021 RID: 4129 RVA: 0x00068D10 File Offset: 0x00066F10
            public ResourceInfo()
            {
                this.Readable = false;
            }

            // Token: 0x1700016C RID: 364
            // (get) Token: 0x06001022 RID: 4130 RVA: 0x00068D20 File Offset: 0x00066F20
            // (set) Token: 0x06001023 RID: 4131 RVA: 0x00068D28 File Offset: 0x00066F28
            [XmlIgnore]
            public ModContainer Mod { get; set; }

            // Token: 0x1700016D RID: 365
            // (get) Token: 0x06001024 RID: 4132 RVA: 0x00068D34 File Offset: 0x00066F34
            // (set) Token: 0x06001025 RID: 4133 RVA: 0x00068D3C File Offset: 0x00066F3C
            
            // Token: 0x1700016E RID: 366
            // (get) Token: 0x06001026 RID: 4134 RVA: 0x00068D48 File Offset: 0x00066F48
            // (set) Token: 0x06001027 RID: 4135 RVA: 0x00068D50 File Offset: 0x00066F50
            [XmlAttribute("name")]
            public string Name { get;  set; }

            // Token: 0x1700016F RID: 367
            // (get) Token: 0x06001028 RID: 4136 RVA: 0x00068D5C File Offset: 0x00066F5C
            // (set) Token: 0x06001029 RID: 4137 RVA: 0x00068D64 File Offset: 0x00066F64
            [XmlAttribute("path")]
            public string Path { get; set; }

            // Token: 0x17000170 RID: 368
            // (get) Token: 0x0600102A RID: 4138 RVA: 0x00068D70 File Offset: 0x00066F70
            // (set) Token: 0x0600102B RID: 4139 RVA: 0x00068D78 File Offset: 0x00066F78
            [XmlAttribute("readable")]
            [DefaultValue(false)]
            public bool Readable { get;  set; }

            // Token: 0x0600102C RID: 4140 RVA: 0x00068D84 File Offset: 0x00066F84
            protected override bool Validate(string name)
            {
                return true;
            }
        }

        // Token: 0x02000284 RID: 644
        [Serializable]
        public class ResourcesChoices : Element
        {
            // Token: 0x0600102D RID: 4141 RVA: 0x00068DC4 File Offset: 0x00066FC4
            public ResourcesChoices()
            {
                this.Resources = new ModInfo.ResourceInfo[0];
                //this.ResourceTypes = new ModResource.ResourceType[0];
            }

            // Token: 0x0600102E RID: 4142 RVA: 0x00068DE4 File Offset: 0x00066FE4
            public static implicit operator List<ModInfo.ResourceInfo>(ModInfo.ResourcesChoices choices)
            {
                return new List<ModInfo.ResourceInfo>(choices.Resources);
            }

            // Token: 0x0400108D RID: 4237
            [XmlChoiceIdentifier("ResourceTypes")]
            [CanBeEmpty]
            [RequireToValidate]
            [XmlElement("Texture", typeof(ModInfo.ResourceInfo))]
            [XmlElement("Mesh", typeof(ModInfo.ResourceInfo))]
            [XmlElement("AudioClip", typeof(ModInfo.ResourceInfo))]
            [XmlElement("AssetBundle", typeof(ModInfo.ResourceInfo))]
            public ModInfo.ResourceInfo[] Resources;

            // Token: 0x0400108E RID: 4238
            
        }

        // Token: 0x02000285 RID: 645
        [Serializable]
        public class TargetChoices : Element
        {
            // Token: 0x0600102F RID: 4143 RVA: 0x00068DF4 File Offset: 0x00066FF4
            public TargetChoices()
            {
                this.Targets = new object[0];
                this.TargetTypes = new ModInfo.TargetChoices.TargetType[0];
            }

            // Token: 0x06001030 RID: 4144 RVA: 0x00068E14 File Offset: 0x00067014
            protected override bool Validate(string elementName)
            {
                return this.Targets.All((object t) => !(t is Element) || ((Element)t).InvokeValidate());
            }

            // Token: 0x0400108F RID: 4239
            [XmlChoiceIdentifier("TargetTypes")]
            
            [XmlElement("AllOfficialEntities", typeof(Element), IsNullable = true)]
            [XmlElement("AllModdedEntities", typeof(Element), IsNullable = true)]
            public object[] Targets;

            // Token: 0x04001090 RID: 4240
            [XmlIgnore]
            public ModInfo.TargetChoices.TargetType[] TargetTypes;

            // Token: 0x02000286 RID: 646
            public enum TargetType
            {
                // Token: 0x04001093 RID: 4243
                Entity,
                // Token: 0x04001094 RID: 4244
                ModdedEntity,
                // Token: 0x04001095 RID: 4245
                AllOfficialEntities,
                // Token: 0x04001096 RID: 4246
                AllModdedEntities
            }
        }

        // Token: 0x02000287 RID: 647
        [Serializable]
        public class TriggerInfo : Element
        {
            // Token: 0x06001032 RID: 4146 RVA: 0x00068E70 File Offset: 0x00067070
            public TriggerInfo()
            {
                this.TargetChoices = new ModInfo.TargetChoices();
                this.LocalId = -1;
            }

            // Token: 0x17000171 RID: 369
            // (get) Token: 0x06001033 RID: 4147 RVA: 0x00068E8C File Offset: 0x0006708C
            // (set) Token: 0x06001034 RID: 4148 RVA: 0x00068E94 File Offset: 0x00067094
            [XmlIgnore]
            public ModContainer Mod { get; set; }

            // Token: 0x17000172 RID: 370
            // (get) Token: 0x06001035 RID: 4149 RVA: 0x00068EA0 File Offset: 0x000670A0
            // (set) Token: 0x06001036 RID: 4150 RVA: 0x00068EA8 File Offset: 0x000670A8
            [XmlElement]
            public string Name { get; set; }

            // Token: 0x17000173 RID: 371
            // (get) Token: 0x06001037 RID: 4151 RVA: 0x00068EB4 File Offset: 0x000670B4
            // (set) Token: 0x06001038 RID: 4152 RVA: 0x00068EBC File Offset: 0x000670BC
            [XmlElement("ID")]
            public int LocalId { get; set; }

            // Token: 0x17000174 RID: 372
            // (get) Token: 0x06001039 RID: 4153 RVA: 0x00068EC8 File Offset: 0x000670C8
            // (set) Token: 0x0600103A RID: 4154 RVA: 0x00068ED0 File Offset: 0x000670D0
            [XmlElement("AvailableOn")]
            [RequireToValidate]
            public ModInfo.TargetChoices TargetChoices { get; set; }
        }

        // Token: 0x02000288 RID: 648
        [XmlRoot("Event")]
        [Serializable]
        public class EventInfo : Element
        {
            // Token: 0x0600103B RID: 4155 RVA: 0x00068EDC File Offset: 0x000670DC
            public EventInfo()
            {
                this.Properties = new EventProperty[0];
                this.XmlProperties = new object[0];
                this.Id = -1;
            }

            // Token: 0x17000175 RID: 373
            // (get) Token: 0x0600103C RID: 4156 RVA: 0x00068F04 File Offset: 0x00067104
            // (set) Token: 0x0600103D RID: 4157 RVA: 0x00068F0C File Offset: 0x0006710C
            [XmlIgnore]
            public ModContainer Mod { get; set; }

            // Token: 0x17000176 RID: 374
            // (get) Token: 0x0600103E RID: 4158 RVA: 0x00068F18 File Offset: 0x00067118
            // (set) Token: 0x0600103F RID: 4159 RVA: 0x00068F20 File Offset: 0x00067120
            [XmlElement]
            public string Name { get; set; }

            // Token: 0x17000177 RID: 375
            // (get) Token: 0x06001040 RID: 4160 RVA: 0x00068F2C File Offset: 0x0006712C
            // (set) Token: 0x06001041 RID: 4161 RVA: 0x00068F34 File Offset: 0x00067134
            [XmlElement("ID")]
            public int Id { get; set; }

            // Token: 0x17000178 RID: 376
            // (get) Token: 0x06001042 RID: 4162 RVA: 0x00068F40 File Offset: 0x00067140
            // (set) Token: 0x06001043 RID: 4163 RVA: 0x00068F48 File Offset: 0x00067148
            [XmlElement("Icon")]
            public ResourceReference IconReference { get; set; }

            // Token: 0x06001044 RID: 4164 RVA: 0x00068F54 File Offset: 0x00067154
            //public void CreateProperties()
            //{
            //    int num = 0;
            //    List<EventProperty> list = new List<EventProperty>();
            //    foreach (object obj in this.XmlProperties)
            //    {
            //        if (obj is EventProperty.Picker)
            //        {
            //            ((EventProperty)obj).Row = -1;
            //            list.Add((EventProperty)obj);
            //        }
            //        else if (obj is ModInfo.EventInfo.PropertyRow)
            //        {
            //            ModInfo.EventInfo.PropertyRow propertyRow = (ModInfo.EventInfo.PropertyRow)obj;
            //            if (propertyRow.Properties != null && propertyRow.Properties.Length != 0)
            //            {
            //                foreach (EventProperty eventProperty in propertyRow.Properties)
            //                {
            //                    eventProperty.Row = num;
            //                    list.Add(eventProperty);
            //                    if (!eventProperty.XSpecified)
            //                    {
            //                        MLog.Warn("EventProperty in a Row without X attribute. Display will likely be broken!");
            //                    }
            //                }
            //                num++;
            //            }
            //        }
            //        else
            //        {
            //            EventProperty eventProperty2 = (EventProperty)obj;
            //            eventProperty2.Row = num;
            //            num++;
            //            list.Add(eventProperty2);
            //            if (eventProperty2.XSpecified)
            //            {
            //                MLog.Warn("X attribute of EventProperty will be ignored if it's not a child of a Row element.");
            //            }
            //        }
            //    }
            //    int num2 = 0;
            //    foreach (EventProperty eventProperty3 in list)
            //    {
            //        if (eventProperty3 is EventProperty.Icon || eventProperty3 is EventProperty.Text)
            //        {
            //            eventProperty3.Name = "gen" + num2;
            //            num2++;
            //        }
            //    }
            //    this.Properties = list.ToArray();
            //}

            // Token: 0x06001045 RID: 4165 RVA: 0x00069108 File Offset: 0x00067308
            public bool Validate(string elementName)
            {
                return true;
            }

            // Token: 0x06001046 RID: 4166 RVA: 0x0006910C File Offset: 0x0006730C
            //internal bool Validate(string elemName, bool fileLoad)
            //{
            //    if (fileLoad && !string.IsNullOrEmpty(this.Path))
            //    {
            //        return base.InvalidData(elemName, "Event loaded from seperate file can't have path attribute!");
            //    }
            //    bool flag = !string.IsNullOrEmpty(this.Path);
            //    bool flag2 = !string.IsNullOrEmpty(this.Name) || this.Id != -1;
            //    if (!flag && !flag2)
            //    {
            //        return base.InvalidData(elemName, "Must contain either a path attribute or the event data.");
            //    }
            //    if (flag && flag2)
            //    {
            //        return base.InvalidData(elemName, "Can't contain both a path attribute and the event data.");
            //    }
            //    if (flag)
            //    {
            //        return true;
            //    }
            //    if (string.IsNullOrEmpty(this.Name))
            //    {
            //        return base.MissingElement(elemName, "Name");
            //    }
            //    if (this.Id == -1)
            //    {
            //        return base.MissingElement(elemName, "ID");
            //    }
            //    return this.Properties.All((EventProperty p) => p.InvokeValidate());
            //}

            // Token: 0x0400109B RID: 4251
            [XmlAttribute("path")]
            public string Path;

            // Token: 0x0400109C RID: 4252
            //[XmlArray("Properties")]
            //[XmlArrayItem("Picker", typeof(EventProperty.Picker))]
            //[XmlArrayItem("TextInput", typeof(EventProperty.TextInput))]
            //[XmlArrayItem("NumberInput", typeof(EventProperty.NumberInput))]
            //[XmlArrayItem("Choice", typeof(EventProperty.Choice))]
            //[XmlArrayItem("Toggle", typeof(EventProperty.Toggle))]
            //[XmlArrayItem("TeamButton", typeof(EventProperty.TeamButton))]
            //[XmlArrayItem("Icon", typeof(EventProperty.Icon))]
            //[XmlArrayItem("Text", typeof(EventProperty.Text))]
            //[XmlArrayItem("Row", typeof(ModInfo.EventInfo.PropertyRow))]
            public object[] XmlProperties;

            // Token: 0x0400109D RID: 4253
            [XmlIgnore]
            public EventProperty[] Properties;

            // Token: 0x02000289 RID: 649
            //[Serializable]
            //public class PropertyRow
            //{
            //    // Token: 0x040010A3 RID: 4259
            //    [XmlElement("Picker", typeof(EventProperty.Picker))]
            //    [XmlElement("TextInput", typeof(EventProperty.TextInput))]
            //    [XmlElement("NumberInput", typeof(EventProperty.NumberInput))]
            //    [XmlElement("Choice", typeof(EventProperty.Choice))]
            //    [XmlElement("Toggle", typeof(EventProperty.Toggle))]
            //    [XmlElement("TeamButton", typeof(EventProperty.TeamButton))]
            //    [XmlElement("Icon", typeof(EventProperty.Icon))]
            //    [XmlElement("Text", typeof(EventProperty.Text))]
            //    public EventProperty[] Properties;
            //}
        }

        // Token: 0x0200028A RID: 650
        [Serializable]
        public class KeyInfo : Element
        {
            // Token: 0x0600104A RID: 4170 RVA: 0x00069224 File Offset: 0x00067424
            protected override bool Validate(string elemName)
            {
                if (this.Name.Contains("|"))
                {
                    return base.InvalidData(elemName, "Key name may not contain '|'!");
                }
                return base.Validate(elemName);
            }

            // Token: 0x040010A4 RID: 4260
            [XmlAttribute("name")]
            public string Name;

            // Token: 0x040010A5 RID: 4261
            [XmlAttribute("defaultModifier")]
            [DefaultValue(KeyCode.None)]
            public KeyCode DefaultModifier;

            // Token: 0x040010A6 RID: 4262
            [XmlAttribute("defaultTrigger")]
            public KeyCode DefaultTrigger;
        }
    }
}
