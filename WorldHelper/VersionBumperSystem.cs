using Unity.Entities;

namespace E7.ECS
{
    /// <summary>
    /// Its purpose of existence is just to increase the global version number on manual update.
    /// </summary>
    [DisableAutoCreation]
    internal class VersionBumperSystem : ComponentSystem
    {
        protected override void OnCreateManager() => this.Enabled = false;
        public void BumpVersion()
        {
            this.Enabled = true;
            Update();
            this.Enabled = false;
            //Debug.Log($"Bumped to {GlobalSystemVersion}");
        }
        protected override void OnUpdate() { }
    }
}