﻿
namespace TheBoxSoftware.Reflection.Core.COFF
{
    using System;
    using System.Collections.Generic;

    public sealed class MetadataStream : Stream
    {
        /// <summary>
        /// <para>
        /// This is stored here because a coded index defenition map is relevant only
        /// for a single pecofffile and by association a metadatastream. If this is static
        /// in coded index (like it was) then the definitions will cause failures in the
        /// application.
        /// </para>
        /// <para>
        /// A map of initialised defenition entries. A definition is valid multiple
        /// times and storing this small structure will increase performance by reducing
        /// the number of row searches and lookups in the metadata tables.
        /// </para>
        /// </summary>
        internal CodedIndexMap codedIndexMap = new CodedIndexMap();
        private byte _sizeOfStringIndexes = 0;
        private byte _sizeOfGuidIndexes = 0;
        private byte _sizeOfBlobIndexes = 0;
        private PeCoffFile _owningFile;
        private MetadataTablesDictionary _tables;
        private HeapOffsetSizes _heapOffsetSizes;

        internal MetadataStream(PeCoffFile file, uint address)
        {
            _owningFile = file;
            byte[] contents = file.FileContents;
            Offset offset = (int)address;

            offset.Shift(4);        // reserved1 = BitConverter.ToUInt32(contents, offset.Shift(4));
            offset.Shift(1);        // majorVersion = contents[offset.Shift(1)];
            offset.Shift(1);        // minorVersion = contents[offset.Shift(1)];
            _heapOffsetSizes = (HeapOffsetSizes)contents.GetValue(offset.Shift(1));
            offset.Shift(1);        // reserved2 = contents[offset.Shift(1)];
            ulong valid = BitConverter.ToUInt64(contents, offset.Shift(8));
            offset.Shift(8);        // sorted = BitConverter.ToUInt64(contents, offset.Shift(8));

            // Now we need to read the number of rows present in the available tables, we have
            // had to add the unused tables to the MEtadatTables as mscorlib seems to use one. Not
            // sure which though.
            Dictionary<MetadataTables, int> rowsInPresentTables = new Dictionary<MetadataTables, int>();
            Array values = Enum.GetValues(typeof(MetadataTables));
            for(int i = 0; i < values.Length - 1; i++)
            {
                MetadataTables current = (MetadataTables)values.GetValue(i);
                ulong mask = (ulong)1 << (int)current;
                if((mask & valid) == mask)
                {
                    rowsInPresentTables.Add(current, BitConverter.ToInt32(contents, offset.Shift(4)));
                }
            }

            ICodedIndexResolver resolver = new CodedIndexResolver(rowsInPresentTables);

            // Following the array of row size we get the actual metadata tables
            _tables = new MetadataTablesDictionary(rowsInPresentTables.Count);
            for(int i = 0; i < values.Length; i++)
            {
                MetadataTables current = (MetadataTables)values.GetValue(i);
                if(!rowsInPresentTables.ContainsKey(current))
                {
                    continue;
                }

                int numRows = rowsInPresentTables[current];
                MetadataRow[] rows = new MetadataRow[numRows];

                switch(current)
                {
                    case MetadataTables.Module:
                        for(int j = 0; j < numRows; j++)
                        {
                            rows[j] = new ModuleMetadataTableRow(contents, offset, SizeOfStringIndexes, SizeOfGuidIndexes);
                        }
                        break;
                    case MetadataTables.TypeRef:
                        for(int j = 0; j < numRows; j++)
                        {
                            rows[j] = new TypeRefMetadataTableRow(contents, offset, resolver, SizeOfStringIndexes);
                        }
                        break;
                    case MetadataTables.TypeDef:
                        for(int j = 0; j < numRows; j++)
                        {
                            int sizeOfMethodIndex = Index.SizeOfIndex(MetadataTables.MethodDef, this);
                            int sizeOfFieldIndex = Index.SizeOfIndex(MetadataTables.Field, this);
                            rows[j] = new TypeDefMetadataTableRow(contents, offset, resolver, sizeOfFieldIndex, sizeOfMethodIndex, SizeOfStringIndexes);
                        }
                        break;
                    case MetadataTables.Field:
                        for(int j = 0; j < numRows; j++)
                        {
                            rows[j] = new FieldMetadataTableRow(contents, offset, SizeOfStringIndexes, SizeOfBlobIndexes);
                        }
                        break;
                    case MetadataTables.MethodDef:
                        for(int j = 0; j < numRows; j++)
                        {
                            int sizeOfParamIndex = Index.SizeOfIndex(MetadataTables.Param, this);
                            rows[j] = new MethodMetadataTableRow(contents, offset, SizeOfStringIndexes, SizeOfBlobIndexes, sizeOfParamIndex);
                        }
                        break;
                    case MetadataTables.Param:
                        for(int j = 0; j < numRows; j++)
                        {
                            rows[j] = new ParamMetadataTableRow(contents, offset, SizeOfStringIndexes);
                        }
                        break;
                    case MetadataTables.InterfaceImpl:
                        for(int j = 0; j < numRows; j++)
                        {
                            int sizeOfTypeDefIndex = Index.SizeOfIndex(MetadataTables.TypeDef, this);
                            rows[j] = new InterfaceImplMetadataTableRow(contents, offset, resolver, sizeOfTypeDefIndex);
                        }
                        break;
                    case MetadataTables.MemberRef:
                        for(int j = 0; j < numRows; j++)
                        {
                            rows[j] = new MemberRefMetadataTableRow(contents, offset, resolver, SizeOfStringIndexes, SizeOfBlobIndexes);
                        }
                        break;
                    case MetadataTables.Constant:
                        for(int j = 0; j < numRows; j++)
                        {
                            rows[j] = new ConstantMetadataTableRow(contents, offset, resolver, SizeOfBlobIndexes);
                        }
                        break;
                    case MetadataTables.CustomAttribute:
                        for(int j = 0; j < numRows; j++)
                        {
                            rows[j] = new CustomAttributeMetadataTableRow(contents, offset, resolver, SizeOfBlobIndexes);
                        }
                        break;
                    case MetadataTables.FieldMarshal:
                        for(int j = 0; j < numRows; j++)
                        {
                            rows[j] = new FieldMarshalMetadataTableRow(contents, offset, resolver, SizeOfBlobIndexes);
                        }
                        break;
                    case MetadataTables.DeclSecurity:
                        for(int j = 0; j < numRows; j++)
                        {
                            rows[j] = new DeclSecurityMetadataTableRow(contents, offset, resolver, SizeOfBlobIndexes);
                        }
                        break;
                    case MetadataTables.ClassLayout:
                        for(int j = 0; j < numRows; j++)
                        {
                            int sizeOfTypeDefIndex = Index.SizeOfIndex(MetadataTables.AssemblyRef, this);
                            rows[j] = new ClassLayoutMetadataTableRow(contents, offset, sizeOfTypeDefIndex);
                        }
                        break;
                    case MetadataTables.FieldLayout:
                        for(int j = 0; j < numRows; j++)
                        {
                            int sizeOfFieldIndex = Index.SizeOfIndex(MetadataTables.Field, this);

                            rows[j] = new FieldLayoutMetadataTableRow(contents, offset, sizeOfFieldIndex);
                        }
                        break;
                    case MetadataTables.StandAloneSig:
                        for(int j = 0; j < numRows; j++)
                        {
                            rows[j] = new StandAloneSigMetadataTableRow(contents, offset, SizeOfBlobIndexes);
                        }
                        break;
                    case MetadataTables.EventMap:
                        for(int j = 0; j < numRows; j++)
                        {
                            int typeDefIndexSize = Index.SizeOfIndex(MetadataTables.TypeDef, this);
                            int eventIndexSize = Index.SizeOfIndex(MetadataTables.Event, this);

                            rows[j] = new EventMapMetadataTableRow(contents, offset, typeDefIndexSize, eventIndexSize);
                        }
                        break;
                    case MetadataTables.Event:
                        for(int j = 0; j < numRows; j++)
                        {
                            rows[j] = new EventMetadataTableRow(contents, offset, resolver, SizeOfStringIndexes);
                        }
                        break;
                    case MetadataTables.PropertyMap:
                        for(int j = 0; j < numRows; j++)
                        {
                            int sizeOfTypeDefIndex = Index.SizeOfIndex(MetadataTables.TypeDef, this);
                            int sizeOfPropertyIndex = Index.SizeOfIndex(MetadataTables.Property, this);

                            rows[j] = new PropertyMapMetadataTableRow(contents, offset, sizeOfTypeDefIndex, sizeOfPropertyIndex);
                        }
                        break;
                    case MetadataTables.Property:
                        for(int j = 0; j < numRows; j++)
                        {
                            rows[j] = new PropertyMetadataTableRow(contents, offset, SizeOfStringIndexes, SizeOfBlobIndexes);
                        }
                        break;
                    case MetadataTables.MethodSemantics:
                        for(int j = 0; j < numRows; j++)
                        {
                            int sizeOfMethodDefIndex = Index.SizeOfIndex(MetadataTables.MethodDef, this);
                            rows[j] = new MethodSemanticsMetadataTableRow(contents, offset, resolver, sizeOfMethodDefIndex);
                        }
                        break;
                    case MetadataTables.MethodImpl:
                        for(int j = 0; j < numRows; j++)
                        {
                            int sizeOfTypeDefIndex = Index.SizeOfIndex(MetadataTables.TypeDef, this);
                            rows[j] = new MethodImplMetadataTableRow(contents, offset, resolver, sizeOfTypeDefIndex);
                        }
                        break;
                    case MetadataTables.ModuleRef:
                        for(int j = 0; j < numRows; j++)
                        {
                            rows[j] = new ModuleRefMetadataTableRow(contents, offset, SizeOfStringIndexes);
                        }
                        break;
                    case MetadataTables.TypeSpec:
                        for(int j = 0; j < numRows; j++)
                        {
                            rows[j] = new TypeSpecMetadataTableRow(this.SizeOfBlobIndexes, contents, offset);
                        }
                        break;
                    case MetadataTables.ImplMap:
                        for(int j = 0; j < numRows; j++)
                        {
                            int sizeOfModuleRefIndex = Index.SizeOfIndex(MetadataTables.ModuleRef, this);
                            rows[j] = new ImplMapMetadataTableRow(contents, offset, resolver, SizeOfStringIndexes, sizeOfModuleRefIndex);
                        }
                        break;
                    case MetadataTables.FieldRVA:
                        for(int j = 0; j < numRows; j++)
                        {
                            int sizeOfFieldIndex = Index.SizeOfIndex(MetadataTables.Field, this);
                            rows[j] = new FieldRVAMetadataTableRow(contents, offset, sizeOfFieldIndex);
                        }
                        break;
                    case MetadataTables.Assembly:
                        for(int j = 0; j < numRows; j++)
                        {
                            rows[j] = new AssemblyMetadataTableRow(contents, offset, SizeOfBlobIndexes, SizeOfStringIndexes);
                        }
                        break;
                    case MetadataTables.AssemblyProcessor:
                        for(int j = 0; j < numRows; j++)
                        {
                            rows[j] = new AssemblyProcessorMetadataTableRow(contents, offset);
                        }
                        break;
                    case MetadataTables.AssemblyOS:
                        for(int j = 0; j < numRows; j++)
                        {
                            rows[j] = new AssemblyOSMetadataTableRow(contents, offset);
                        }
                        break;
                    case MetadataTables.AssemblyRef:
                        for(int j = 0; j < numRows; j++)
                        {
                            rows[j] = new AssemblyRefMetadataTableRow(contents, offset, SizeOfBlobIndexes, SizeOfStringIndexes);
                        }
                        break;
                    case MetadataTables.AssemblyRefProcessor:
                        for(int j = 0; j < numRows; j++)
                        {
                            int sizeOfAssemblyRefIndex = Index.SizeOfIndex(MetadataTables.AssemblyRef, this);
                            rows[j] = new AssemblyRefProcessorMetadataTableRow(contents, offset, sizeOfAssemblyRefIndex);
                        }
                        break;
                    case MetadataTables.AssemblyRefOS:
                        for(int j = 0; j < numRows; j++)
                        {
                            int sizeOfAssemblyRefIndex = Index.SizeOfIndex(MetadataTables.AssemblyRef, this);
                            rows[j] = new AssemblyRefOSMetadataTableRow(contents, offset, sizeOfAssemblyRefIndex);
                        }
                        break;
                    case MetadataTables.File:
                        for(int j = 0; j < numRows; j++)
                        {
                            rows[j] = new FileMetadataTableRow(contents, offset, SizeOfBlobIndexes, SizeOfStringIndexes);
                        }
                        break;
                    case MetadataTables.ExportedType:
                        for(int j = 0; j < numRows; j++)
                        {
                            rows[j] = new ExportedTypeMetadataTableRow(contents, offset, resolver, SizeOfStringIndexes);
                        }
                        break;
                    case MetadataTables.ManifestResource:
                        for(int j = 0; j < numRows; j++)
                        {
                            rows[j] = new ManifestResourceMetadataTableRow(contents, offset, resolver, SizeOfStringIndexes);
                        }
                        break;
                    case MetadataTables.NestedClass:
                        for(int j = 0; j < numRows; j++)
                        {
                            int sizeOfTypeDefIndex = Index.SizeOfIndex(MetadataTables.TypeDef, this);
                            rows[j] = new NestedClassMetadataTableRow(contents, offset, sizeOfTypeDefIndex);
                        }
                        break;
                    case MetadataTables.GenericParam:
                        for(int j = 0; j < numRows; j++)
                        {
                            rows[j] = new GenericParamMetadataTableRow(contents, offset, resolver, SizeOfStringIndexes);
                        }
                        break;
                    case MetadataTables.MethodSpec:
                        for(int j = 0; j < numRows; j++)
                        {
                            rows[j] = new MethodSpecMetadataTableRow(contents, offset, resolver, SizeOfBlobIndexes);
                        }
                        break;
                    case MetadataTables.GenericParamConstraint:
                        for(int j = 0; j < numRows; j++)
                        {
                            int sizeOfGenericParamIndex = Index.SizeOfIndex(MetadataTables.GenericParam, this);
                            rows[j] = new GenericParamConstraintMetadataTableRow(contents, offset, resolver, sizeOfGenericParamIndex);
                        }
                        break;
                }

                _tables.SetMetadataTable(current, rows);
            }
        }

        /// <summary>
        /// Obtains an entry in the specified table at the specified index
        /// </summary>
        /// <param name="codedIndex">The coded index decribing the metadata location.</param>
        /// <returns>The MetadataTableRow or null if not found</returns>
        public MetadataRow GetEntryFor(CodedIndex codedIndex)
        {
            return GetEntryFor(codedIndex.Table, codedIndex.Index);
        }

        /// <summary>
        /// Obtains an entry in the specified table at the specified index
        /// </summary>
        /// <param name="table">The table to get the metadata for</param>
        /// <param name="index">The index in the table</param>
        /// <returns>The MetadataTableRow or null if not found</returns>
        public MetadataRow GetEntryFor(MetadataTables table, uint index)
        {
            MetadataRow o = null;
            if(index <= _tables[table].Length)
            {
                o = _tables[table][index - 1];
            }
            return o;
        }

        public MetadataTablesDictionary Tables
        {
            get { return _tables; }
            set { _tables = value; }
        }

        /// <summary>
        /// Returns the size (in bytes) of the indexes to the string heap
        /// </summary>
        public byte SizeOfStringIndexes
        {
            get
            {
                if(_sizeOfStringIndexes == 0)
                {
                    _sizeOfStringIndexes = ((_heapOffsetSizes & HeapOffsetSizes.StringIsLarge) == HeapOffsetSizes.StringIsLarge)
                        ? (byte)4
                        : (byte)2;
                }
                return _sizeOfStringIndexes;
            }
        }

        /// <summary>
        /// Returns the size (in bytes) of the indexes to the guid heap
        /// </summary>
        public byte SizeOfGuidIndexes
        {
            get
            {
                if(_sizeOfGuidIndexes == 0)
                {
                    _sizeOfGuidIndexes = ((_heapOffsetSizes & HeapOffsetSizes.GuidIsLarge) == HeapOffsetSizes.GuidIsLarge)
                        ? (byte)4
                        : (byte)2;
                }
                return _sizeOfGuidIndexes;
            }
        }

        /// <summary>
        /// Returns the size (in bytes) of the indexes to the blob heap
        /// </summary>
        public byte SizeOfBlobIndexes
        {
            get
            {
                if(_sizeOfBlobIndexes == 0)
                {
                    _sizeOfBlobIndexes = ((_heapOffsetSizes & HeapOffsetSizes.BlobIsLarge) == HeapOffsetSizes.BlobIsLarge)
                        ? (byte)4
                        : (byte)2;
                }
                return _sizeOfBlobIndexes;
            }
        }

        public PeCoffFile OwningFile
        {
            get { return _owningFile; }
            private set { _owningFile = value; }
        }
    }
}