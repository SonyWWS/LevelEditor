//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.


using System;
using System.IO;

using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Dom;

using LevelEditorCore;

namespace LevelEditor.DomNodeAdapters
{
    /// <summary>
    /// Reference to sub game.
    /// </summary>
    public class GameReference : DomNodeAdapter, 
        IReference<IGame>,         
        IReference<IGameDocument>,
        IListable
    {
        public static GameReference CreateNew(Uri uri)
        {
            string fileName = System.IO.Path.GetFileName(uri.LocalPath);
            DomNode dom = new DomNode(Schema.gameReferenceType.Type);
            dom.SetAttribute(Schema.gameReferenceType.nameAttribute, fileName);
            dom.SetAttribute(Schema.gameReferenceType.refAttribute, uri);
            GameReference gameRef = dom.As<GameReference>();
            return gameRef;
        }

        public static GameReference CreateNew(GameDocument subDoc)
        {
            if (subDoc == null)
                throw new ArgumentNullException("subDoc");

            string fileName = System.IO.Path.GetFileName(subDoc.Uri.LocalPath);
            DomNode gameRefNode = new DomNode(Schema.gameReferenceType.Type);
            gameRefNode.SetAttribute(Schema.gameReferenceType.nameAttribute, fileName);
            gameRefNode.SetAttribute(Schema.gameReferenceType.refAttribute, subDoc.Uri);
            GameReference gameRef = gameRefNode.As<GameReference>();
            gameRef.m_target = subDoc.Cast<IGame>();
            subDoc.Cast<Game>().SetParent(gameRef);
            gameRef.m_error = string.Empty;
            return gameRef;
        }

        #region IReference<Game> Members

        public bool CanReference(IGame item)
        {
            return false;
        }

        public IGame Target
        {
            get { return m_target; }
            set { throw new InvalidOperationException("Target cannot be set"); }
        }

        #endregion

        #region IReference<GameDocument> Members

        bool IReference<IGameDocument>.CanReference(IGameDocument item)
        {
            return false;
        }

        IGameDocument IReference<IGameDocument>.Target
        {
            get { return Adapters.As<GameDocument>(m_target); }
            set { throw new InvalidOperationException("Target cannot be set"); }
        }

        #endregion

        #region IListable Members

        /// <summary>
        /// Provides info for the ProjectLister tree view and other controls</summary>
        /// <param name="info">Item info passed in and modified by the method</param>
        public void GetInfo(ItemInfo info)
        {
            info.ImageIndex = (m_target != null)
                                  ? info.GetImageList().Images.IndexOfKey(LevelEditorCore.Resources.FolderRefImage)
                                  : info.GetImageList().Images.IndexOfKey(
                                      LevelEditorCore.Resources.MissingFolderRefImage);
            IDocument gameDoc = Adapters.As<IDocument>(m_target);

            string name = GetAttribute<string>(Schema.gameReferenceType.nameAttribute);
            if (name == null) name = string.Empty;
            if (gameDoc != null && gameDoc.Dirty)
                name += "*";

            if (m_target == null && !string.IsNullOrEmpty(m_error))
            {
                name += " [ Error: " + m_error + " ]";
            }

            info.Label = name;
            info.IsLeaf = m_target == null;
        }

        #endregion

        /// <summary>
        /// Gets absolute uri of the target GameDocument.</summary>
        public Uri Uri
        {
            get { return GetAttribute<Uri>(Schema.gameReferenceType.refAttribute); }
        }

        /// <summary>
        /// Resolves the Uri to DomNode</summary>        
        public void Resolve()
        {
            if (m_target == null)
            {
                var gameDocRegistry = Globals.MEFContainer.GetExportedValue<GameDocumentRegistry>();

                Uri ur = Uri;
                if (ur == null)
                {
                    m_error = "ref attribute is null";
                }
                else if (!File.Exists(ur.LocalPath))
                {
                    m_error = "File not found: " + ur.LocalPath;
                }
                else if (gameDocRegistry.FindDocument(ur) != null)
                {
                    m_error = "Causes circular ref: " + ur.LocalPath;
                }
                else
                {
                    SchemaLoader schemaloader = Globals.MEFContainer.GetExportedValue<SchemaLoader>();
                    GameDocument gameDoc = GameDocument.OpenOrCreate(ur, schemaloader);
                    m_target = gameDoc.As<IGame>();
                    ((Game)m_target).SetParent(this);
                }
            }            
        }

        /// <summary>
        /// Sets target to null and removes the resolved 
        /// GameDocument from  GameDocumentRegistry</summary>
        public void Unresolve()
        {
            if (m_target != null)
            {
                var gameDocRegistry = Globals.MEFContainer.GetExportedValue<GameDocumentRegistry>();
                gameDocRegistry.Remove(m_target.As<IGameDocument>());
                ((Game)m_target).SetParent(null);
                m_target = null;

                m_error = "Not resolved";
            }            
        }

        /// <summary>
        /// Gets a value indicating if this reference has been resolved</summary>        
        public bool Resolved
        {
            get { return m_target != null; }
        }
                
        private string m_error = string.Empty;
        private IGame m_target = null;

    }
}