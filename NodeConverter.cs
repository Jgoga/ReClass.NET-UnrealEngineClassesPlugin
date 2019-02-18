﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using ReClassNET.DataExchange.ReClass;
using ReClassNET.Logger;
using ReClassNET.Nodes;
using UnrealEngineClassesPlugin.Nodes;

namespace UnrealEngineClassesPlugin
{
	public class NodeConverter : ICustomNodeSerializer
	{
		/// <summary>Name of the type used in the XML data.</summary>
		private const string XmlTypePrefix = "UnrealEngineClasses.";

		private static readonly Dictionary<string, Type> stringToTypeMap = new[]
		{
			typeof(FDateTimeNode),
			typeof(FGuidNode),
			typeof(FQWordNode),
			typeof(TArrayNode),
			typeof(TSharedPtrNode)
		}.ToDictionary(t => XmlTypePrefix + t.Name, t => t);

		private static readonly Dictionary<Type, string> typeToStringMap = stringToTypeMap.ToDictionary(kv => kv.Value, kv => kv.Key);

		public bool CanHandleNode(BaseNode node) => node is FDateTimeNode || node is FGuidNode || node is FQWordNode || node is FStringNode || node is TArrayNode || node is TSharedPtrNode;

		public bool CanHandleElement(XElement element) => element.Attribute(ReClassNetFile.XmlTypeAttribute)?.Value.StartsWith(XmlTypePrefix) == true;

		public BaseNode CreateNodeFromElement(XElement element, BaseNode parent, IEnumerable<ClassNode> classes, ILogger logger, CreateNodeFromElementHandler createNodeFromElement)
		{
			if (!stringToTypeMap.TryGetValue(element.Attribute(ReClassNetFile.XmlTypeAttribute)?.Value ?? string.Empty, out var nodeType))
			{
				logger.Log(LogLevel.Error, $"Skipping node with unknown type: {element.Attribute(ReClassNetFile.XmlTypeAttribute)?.Value}");
				logger.Log(LogLevel.Warning, element.ToString());

				return null;
			}

			return BaseNode.CreateInstanceFromType(nodeType, false);
		}

		public XElement CreateElementFromNode(BaseNode node, ILogger logger, CreateElementFromNodeHandler defaultHandler)
		{
			var element = new XElement(
				ReClassNetFile.XmlNodeElement,
				new XAttribute(ReClassNetFile.XmlNameAttribute, node.Name ?? string.Empty),
				new XAttribute(ReClassNetFile.XmlCommentAttribute, node.Comment ?? string.Empty)
			);

			switch (node)
			{
				case FDateTimeNode _:
					element.SetAttributeValue(ReClassNetFile.XmlTypeAttribute, XmlTypePrefix + "FDateTime");
					break;
				case FGuidNode _:
					element.SetAttributeValue(ReClassNetFile.XmlTypeAttribute, XmlTypePrefix + "FGuid");
					break;
				case FQWordNode _:
					element.SetAttributeValue(ReClassNetFile.XmlTypeAttribute, XmlTypePrefix + "FQWord");
					break;
				case FStringNode _:
					element.SetAttributeValue(ReClassNetFile.XmlTypeAttribute, XmlTypePrefix + "FString");
					break;
				case TArrayNode _:
					element.SetAttributeValue(ReClassNetFile.XmlTypeAttribute, XmlTypePrefix + "TArray");
					break;
				case TSharedPtrNode _:
					element.SetAttributeValue(ReClassNetFile.XmlTypeAttribute, XmlTypePrefix + "TSharedPtr");
					break;
			}

			if (node is BaseWrapperNode wrapperNode)
			{
				element.Add(defaultHandler(wrapperNode.InnerNode, logger));
			}

			return element;
		}
	}
}
