﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheBoxSoftware.Reflection;
using TheBoxSoftware.Reflection.Comments;

namespace TheBoxSoftware.Documentation.Exporting.Rendering {
	class PropertyXmlRenderer : XmlRenderer {
		private PropertyDef member;
		private XmlCodeCommentFile xmlComments;

		/// <summary>
		/// Initializes a new instance of the <see cref="PropertyXmlRenderer"/> class.
		/// </summary>
		/// <param name="entry">The entry to intialise the renderer with.</param>
		public PropertyXmlRenderer(Entry entry) {
			this.member = (PropertyDef)entry.Item;
			this.xmlComments = entry.XmlCommentFile;
			this.AssociatedEntry = entry;
		}

		public override void Render(System.Xml.XmlWriter writer) {
			CRefPath crefPath = new CRefPath(this.member);
			XmlCodeComment comment = this.xmlComments.ReadComment(crefPath);

			writer.WriteStartElement("member");
			writer.WriteAttributeString("id", this.AssociatedEntry.Key.ToString());
			writer.WriteAttributeString("subId", this.AssociatedEntry.SubKey);
			writer.WriteStartElement("name");
			writer.WriteString(this.member.GetDisplayName(false));
			writer.WriteEndElement();

			// find and output the summary
			if (comment != XmlCodeComment.Empty) {
				XmlCodeElement summary = comment.Elements.Find(currentBlock => currentBlock is SummaryXmlCodeElement);
				if (summary != null) {
					this.Serialize(summary, writer);
				}
			}

			this.RenderSyntaxBlocks(this.member, writer);

			// find and output the summary
			if (comment != XmlCodeComment.Empty) {
				XmlCodeElement remarks = comment.Elements.Find(currentBlock => currentBlock is RemarksXmlCodeElement);
				if (remarks != null) {
					this.Serialize(remarks, writer);
				}
			}

			this.RenderSeeAlsoBlock(member, writer, comment);

			writer.WriteEndElement();
		}
	}
}
