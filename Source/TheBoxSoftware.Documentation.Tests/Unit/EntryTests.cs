﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using TheBoxSoftware.Reflection.Comments;

namespace TheBoxSoftware.Documentation.Tests.Unit
{
    [TestFixture]
    public class EntryTests
    {
        private Entry CreateEntry(long key, string subkey)
        {
            Entry entry = new Entry(null, "display name", null);
            entry.Key = key;
            entry.SubKey = subkey;
            return entry;
        }

        [Test]
        public void Entry_FindByKey_WhenCurrentItemIsSearched_ReturnsItself()
        {
            const long KEY = 1;
            const string SUBKEY = "test";

            Entry entry = CreateEntry(KEY, SUBKEY);

            Entry found = entry.FindByKey(KEY, SUBKEY);

            Assert.AreEqual(entry, found);
        }

        [Test]
        public void Entry_FindByKey_WhenChildIsSearched_ReturnsCorrectResult()
        {
            const long KEY = 1;
            const string SUBKEY = "test";

            Entry entry = CreateEntry(2, string.Empty);
            Entry child = CreateEntry(KEY, SUBKEY);
            entry.Children.Add(child);

            Entry found = entry.FindByKey(KEY, SUBKEY);

            Assert.AreEqual(child, found);
        }

        [Test]
        public void Entry_FindByKey_WhenChildOfChildIsSearched_ReturnsCorrectResult()
        {
            const long KEY = 1;
            const string SUBKEY = "test";

            Entry entry = CreateEntry(2, string.Empty);
            Entry firstChild = CreateEntry(3, string.Empty);
            Entry secondChild = CreateEntry(KEY, SUBKEY);

            firstChild.Children.Add(secondChild);
            entry.Children.Add(firstChild);

            Entry found = entry.FindByKey(KEY, SUBKEY);

            Assert.AreEqual(secondChild, found);
        }

        [Test]
        public void Entry_FindByKey_WhenNoChildrenAndSearchDoesntMatch_ReturnsNull()
        {
            Entry entry = CreateEntry(1, string.Empty);

            Entry found = entry.FindByKey(2, null);

            Assert.IsNull(found);
        }

        [Test]
        public void Entry_FindByKey_WhenHasChildEntriesAndCheckChildrenIsFalse_DoesntSearchChildren()
        {
            Entry parent = CreateEntry(1, "parent");
            Entry child = CreateEntry(2, string.Empty);

            parent.Children.Add(child);

            Entry found = parent.FindByKey(2, string.Empty, false);

            Assert.IsNull(found);
        }

        [Test]
        public void Entry_FindByKey_WhenSubKeyIsNullAndSearchStringEmpty_ReturnsResult()
        {
            Entry parent = CreateEntry(1, null);

            Entry found = parent.FindByKey(1, string.Empty);

            Assert.AreEqual(found, parent);
        }

        [Test]
        public void Entry_Search_WhenSearchingForParent_ReturnsResult()
        {
            WhenSearchingShouldMatch("InputFileReader");
        }

        [Test]
        public void Entry_Search_WhenSearching_SearchIsCaseInsensative()
        {
            WhenSearchingShouldMatch("INPUTFILEREADER");
        }

        [Test]
        public void Entry_Search_WhenSearching_ShouldMatchPartial()
        {
            WhenSearchingShouldMatch("FILE");
        }

        private void WhenSearchingShouldMatch(string searchFor)
        {
            Entry parent = CreateEntry(1, string.Empty);
            parent.Name = "InputFileReader";
            parent.IsSearchable = true;

            List<Entry> found = parent.Search(searchFor);

            Assert.AreEqual(1, found.Count);
            Assert.AreEqual(found[0], parent);
        }
    }
}