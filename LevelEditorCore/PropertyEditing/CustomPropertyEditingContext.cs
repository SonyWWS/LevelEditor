//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Controls.PropertyEditing;

namespace LevelEditorCore.PropertyEditing
{
    public class CustomPropertyEditingContext : PropertyEditingContext, IAdaptable
    {

        public CustomPropertyEditingContext(object item)
            : base(new[]{ item })
        {
        }

        #region IAdaptable Members

        object IAdaptable.GetAdapter(Type type)
        {                       
            if (type == typeof(ITransactionContext))
            {
                IGameDocumentRegistry docreg = Globals.MEFContainer.GetExportedValue<IGameDocumentRegistry>();
                return docreg.MasterDocument.As<ITransactionContext>();
            }
            return null;
        }

        #endregion
    }
}
