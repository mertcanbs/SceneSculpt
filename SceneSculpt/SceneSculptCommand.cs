using Rhino;
using Rhino.Commands;

namespace SceneSculpt
{
	public class SceneSculptCommand : Command
	{
		public SceneSculptCommand()
		{
			// Rhino only creates one instance of each command class defined in a
			// plug-in, so it is safe to store a refence in a static property.
			Instance = this;
		}

		///<summary>The only instance of this command.</summary>
		public static SceneSculptCommand Instance { get; private set; }

		///<returns>The command name as it appears on the Rhino command line.</returns>
		public override string EnglishName => "SceneSculptCommand";

		protected override Result RunCommand(RhinoDoc doc, RunMode mode)
		{
			var form = new SceneSculptForm();
			form.Show();
			return Result.Success;
		}
	}
}

