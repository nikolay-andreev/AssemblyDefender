using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AssemblyDefender.Net;
using AssemblyDefender.Common.Collections;
using AssemblyDefender.Common.Cryptography;
using AssemblyDefender.Common.IO;

namespace AssemblyDefender
{
    public class Project
    {
        #region Fields

        public static readonly string FileExtension = ".adproj";
        private static readonly string HeaderSignature = "ADPROJ";
        private string _location;
        private Guid _projectID;
        private DateTime _createdDate;
        private DateTime _lastModifiedDate;
        private int _flags;
        private List<ProjectAssembly> _assemblies;

        #endregion

        #region Ctors

        public Project()
        {
            _projectID = Guid.NewGuid();
            _createdDate = DateTime.Now;
            _assemblies = new List<ProjectAssembly>();
        }

        private Project(IBinaryAccessor accessor, ProjectReadState state)
        {
            Read(accessor, state);
        }

        #endregion

        #region Properties

        public string Location
        {
            get { return _location; }
        }

        public Guid ProjectID
        {
            get { return _projectID; }
        }

        public DateTime CreatedDate
        {
            get { return _createdDate; }
        }

        public DateTime LastModifiedDate
        {
            get { return _lastModifiedDate; }
        }

        public List<ProjectAssembly> Assemblies
        {
            get { return _assemblies; }
        }

        #endregion

        #region Methods

        public void Scavenge()
        {
            foreach (var assembly in _assemblies)
            {
                assembly.Scavenge();
            }
        }

        public void SaveFile(string filePath)
        {
            using (var accessor = new StreamAccessor(new FileStream(filePath, FileMode.Create, FileAccess.Write)))
            {
                Save(accessor, Path.GetDirectoryName(filePath));
            }
        }

        public void Save(IBinaryAccessor accessor, string basePath)
        {
            var state = new ProjectWriteState();
            state.BasePath = basePath;
            state.Strings = new HashList<string>(50);
            state.Signatures = new WriteSignatureSerializer(100, state.Strings);

            accessor.Write((string)HeaderSignature, Encoding.UTF8);
            accessor.Write((short)0);
            accessor.Write((Guid)_projectID);
            accessor.Write((DateTime)_createdDate);
            accessor.Write((DateTime)DateTime.Now);
            accessor.Write((int)_flags);

            var blob = new Blob();
            WriteAssemblies(new BlobAccessor(blob), state);
            WriteStrings(accessor, state);
            WriteSignatures(accessor, state);
            accessor.Write(blob.GetBuffer(), 0, blob.Length);
        }

        private void WriteAssemblies(IBinaryAccessor accessor, ProjectWriteState state)
        {
            accessor.Write7BitEncodedInt(_assemblies.Count);

            foreach (var assembly in _assemblies)
            {
                assembly.Write(accessor, state);
            }
        }

        private void WriteStrings(IBinaryAccessor accessor, ProjectWriteState state)
        {
            var strings = state.Strings;

            int count = strings.Count;

            var blob = new Blob();
            int pos = 0;

            blob.Write7BitEncodedInt(ref pos, count);

            var encoding = Encoding.UTF8;

            for (int i = 0; i < count; i++)
            {
                blob.WriteLengthPrefixedString(ref pos, strings[i], encoding);
            }

            StrongCryptoUtils.Encrypt(blob.GetBuffer(), 0, blob.Length);

            accessor.Write7BitEncodedInt(blob.Length);
            accessor.Write(blob.GetBuffer(), 0, blob.Length);
        }

        private void WriteSignatures(IBinaryAccessor accessor, ProjectWriteState state)
        {
            byte[] buffer = state.Signatures.Save();
            accessor.Write7BitEncodedInt(buffer.Length);
            accessor.Write(buffer, 0, buffer.Length);
        }

        private void Read(IBinaryAccessor accessor, ProjectReadState state)
        {
            _projectID = accessor.ReadGuid();
            _createdDate = accessor.ReadDateTime();
            _lastModifiedDate = accessor.ReadDateTime();
            _flags = accessor.ReadInt32();

            ReadStrings(accessor, state);
            ReadSignatures(accessor, state);
            ReadAssemblies(accessor, state);
        }

        private void ReadAssemblies(IBinaryAccessor accessor, ProjectReadState state)
        {
            int count = accessor.Read7BitEncodedInt();
            if (count > 0)
            {
                _assemblies = new List<ProjectAssembly>(count);

                for (int i = 0; i < count; i++)
                {
                    _assemblies.Add(new ProjectAssembly(accessor, state));
                }
            }
            else
            {
                _assemblies = new List<ProjectAssembly>();
            }
        }

        private void ReadStrings(IBinaryAccessor accessor, ProjectReadState state)
        {
            int blobSize = accessor.Read7BitEncodedInt();
            byte[] buffer = accessor.ReadBytes(blobSize);
            StrongCryptoUtils.Decrypt(buffer, 0, blobSize);

            var blob = new Blob(buffer);
            int pos = 0;

            int count = blob.Read7BitEncodedInt(ref pos);
            var strings = new string[count];
            var encoding = Encoding.UTF8;

            for (int i = 0; i < count; i++)
            {
                strings[i] = blob.ReadLengthPrefixedString(ref pos, encoding);
            }

            state.Strings = strings;
        }

        private void ReadSignatures(IBinaryAccessor accessor, ProjectReadState state)
        {
            int blobSize = accessor.Read7BitEncodedInt();
            var blob = new Blob(accessor.ReadBytes(blobSize));
            state.Signatures = new ReadSignatureSerializer(new BlobAccessor(blob), state.Strings);
        }

        #endregion

        #region Static

        public static Project LoadFile(string filePath, bool throwOnError = false)
        {
            using (var accessor = new StreamAccessor(new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read)))
            {
                try
                {
                    var project = Read(accessor, Path.GetDirectoryName(filePath));
                    project._location = filePath;

                    return project;
                }
                catch (Exception)
                {
                    if (throwOnError)
                    {
                        throw new ProjectException(string.Format(SR.ProjectFileNotValid, filePath));
                    }

                    return null;
                }
            }
        }

        public static Project Load(IBinaryAccessor accessor, string basePath, bool throwOnError = false)
        {
            try
            {
                return Read(accessor, basePath);
            }
            catch (Exception)
            {
                if (throwOnError)
                {
                    throw new ProjectException(SR.ProjectNotValid);
                }

                return null;
            }
        }

        private static Project Read(IBinaryAccessor accessor, string basePath)
        {
            if (HeaderSignature != accessor.ReadString(6, Encoding.UTF8))
            {
                throw new ProjectException(SR.ProjectNotValid);
            }

            accessor.Position += 2;

            var state = new ProjectReadState();
            state.BasePath = basePath;

            return new Project(accessor, state);
        }

        #endregion

        #region Nested types

        private class ReadSignatureSerializer : SignatureSerializer
        {
            private string[] _strings;

            internal ReadSignatureSerializer(IBinaryAccessor accessor, string[] strings)
                : base(accessor)
            {
                _strings = strings;
            }

            protected override string ReadString(ref int pos)
            {
                int rid = _blob.Read7BitEncodedInt(ref pos);
                if (rid == 0)
                    return null;

                return _strings[rid - 1];
            }

            protected override void WriteString(ref int pos, string value)
            {
                throw new InvalidOperationException();
            }
        }

        private class WriteSignatureSerializer : SignatureSerializer
        {
            private HashList<string> _strings;

            internal WriteSignatureSerializer(int capacity, HashList<string> strings)
                : base(capacity)
            {
                _strings = strings;
            }

            protected override string ReadString(ref int pos)
            {
                int rid = _blob.Read7BitEncodedInt(ref pos);
                if (rid == 0)
                    return null;

                return _strings[rid - 1];
            }

            protected override void WriteString(ref int pos, string value)
            {
                if (value != null)
                    _blob.Write7BitEncodedInt(ref pos, _strings.Set(value) + 1);
                else
                    _blob.Write(ref pos, (byte)0);
            }
        }

        #endregion
    }
}
