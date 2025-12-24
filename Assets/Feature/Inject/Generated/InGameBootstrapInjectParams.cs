// このファイルは自動生成されています。編集しないでください。
using UnityEngine;

namespace DominantK.Core
{
    public partial class InGameBootstrap
    {
        /// <summary>
        /// 注入パラメータを適用
        /// </summary>
        private void ApplyInjectParams()
        {
            if (!InjectSystem.IsParamInjectSettingsAvailable()) return;
            var settings = InjectSystem.ParamInjectSettingsProperty;
            if (settings?.SelectedParamList == null) return;
            var paramList = settings.SelectedParamList;

            gridWidth = paramList.GridWidth;
            gridHeight = paramList.GridHeight;
        }
    }
}
