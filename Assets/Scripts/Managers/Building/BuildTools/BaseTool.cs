using System;
using System.Collections.Generic;
using UnityEngine;

namespace BuildTools
{
    [System.Serializable]
    public class BaseTool
    {
        protected BuildingManager builder;
        public BuildingManager.ToolMode mode;
        public BuildingManagerUI.BuildToolUI ui { get; set; }
        
        public virtual void Init(BuildingManager builder)
        {
            this.builder = builder;
            ui = this.builder.ui.FetchBuildToolUI(mode);
        }

        public virtual void SetActive(bool active)
        {
            if (active)
            {
                ui.SetOpen();
            }
            else
            {
                ui.SetClosed();
            }
        }

        public void SetIncrementSize(float value)
        {
            
        }
    }
}
