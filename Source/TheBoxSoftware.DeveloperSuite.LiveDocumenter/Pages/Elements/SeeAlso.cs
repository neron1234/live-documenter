﻿
namespace TheBoxSoftware.DeveloperSuite.LiveDocumenter.Pages.Elements
{
    using System.Windows.Documents;
    using TheBoxSoftware.Reflection.Comments;

    /// <summary>
    /// Represents a link that is to be displayed in a see also section
    /// of the document. This currently relates to the seealso xml code
    /// comment element.
    /// </summary>
    public sealed class SeeAlso : Hyperlink
    {
        /// <summary>
        /// Initialises a SeeAlso instance.
        /// </summary>
        /// <param name="key">The <see cref="CRefPath"/> to the type being refered to.</param>
        /// <param name="name">The display name of the SeeAlso referenced member</param>
        internal SeeAlso(CrefEntryKey key, string name) : base()
        {
            this.Resources.MergedDictionaries.Add(DocumentationResources.BaseResources);
            this.Name = name;
            this.Inlines.Add(new Run(name));
            if(key != null)
            {
                this.Tag = key;
                this.Click += new System.Windows.RoutedEventHandler(LinkHelper.Resolve);
            }
            else
            {
                this.IsEnabled = false;
            }
        }

        /// <summary>
        /// Initialises a new instance of the SeeAlso class.
        /// </summary>
        /// <param name="name">The display name for the referenced memeber</param>
        internal SeeAlso(string name) : this(null, name) { }

        /// <summary>
        /// Gets the name of the member being referenced.
        /// </summary>
        internal new string Name { get; private set; }

        /// <summary>
        /// Creats a new SeeAlso element using the same information as this instance.
        /// </summary>
        /// <returns>A new SeeAlso reference</returns>
        internal SeeAlso Clone()
        {
            return new SeeAlso(this.Tag as CrefEntryKey, this.Name);
        }
    }
}