using System.Drawing;
using Thermo.Framework.Core;
using Thermo.SampleManager.Common.Data;
using Thermo.SampleManager.Library.EntityDefinition;
using Thermo.SampleManager.Server;

namespace Thermo.SampleManager.ObjectModel
{
	/// <summary>
	/// Defines extended business logic and manages access to the ICON entity.
	/// </summary>
	[SampleManagerEntity(EntityName)]
	public class Icon : IconBase
	{
		#region Properties

		/// <summary>
		/// Gets the Icon Image
		/// </summary>
		/// <value>
		/// The image.
		/// </value>
		[PromptImage]
		public Image Image
		{
			get
			{
				if (!IsValid()) return null;
				IIconService iconService = Library.GetService<IIconService>();
				return iconService.LoadImage(new IconName(Identity), 16);
			}
		}

		/// <summary>
		/// Gets the Icon Image (24) 
		/// </summary>
		/// <value>
		/// The image.
		/// </value>
		[PromptImage]
		public Image Image24
		{
			get
			{
				if (!IsValid()) return null;
				IIconService iconService = Library.GetService<IIconService>();
				return iconService.LoadImage(new IconName(Identity), 24);
			}
		}

		/// <summary>
		/// Gets the Icon Image (32) 
		/// </summary>
		/// <value>
		/// The image.
		/// </value>
		[PromptImage]
		public Image Image32
		{
			get
			{
				if (!IsValid()) return null;
				IIconService iconService = Library.GetService<IIconService>();
				return iconService.LoadImage(new IconName(Identity), 32);
			}
		}

		/// <summary>
		/// Gets the Icon Image (48) 
		/// </summary>
		/// <value>
		/// The image.
		/// </value>
		[PromptImage]
		public Image Image48
		{
			get
			{
				if (!IsValid()) return null;
				IIconService iconService = Library.GetService<IIconService>();
				return iconService.LoadImage(new IconName(Identity), 48);
			}
		}

		#endregion
	}
}
