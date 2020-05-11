using com.sun.codemodel.@internal.fmt;
using com.sun.org.apache.xml.@internal.resolver.helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ThreeRingsSharp.Utility.Interface {

	/// <summary>
	/// Handles click events for <see cref="DataTreeObject"/> by storing bindings from a <see cref="DataTreeObject"/> to its identical <see cref="TreeNode"/>.
	/// </summary>
	public class DataTreeObjectEventMarshaller {

		private static readonly Dictionary<TreeNode, DataTreeObject> NodeBindings = new Dictionary<TreeNode, DataTreeObject>();
		/// <summary>
		/// Erases all node bindings created via <see cref="RegisterTreeNodeBinding(DataTreeObject, TreeNode)"/>.
		/// </summary>
		public static void ClearAllNodeBindings() {
			NodeBindings.Clear();
		}

		/// <summary>
		/// Registers a binding from a <see cref="DataTreeObject"/> to a <see cref="TreeNode"/>.
		/// </summary>
		/// <param name="data"></param>
		/// <param name="node"></param>
		public static void RegisterTreeNodeBinding(DataTreeObject data, TreeNode node) {
			NodeBindings[node] = data;
		}

		/// <summary>
		/// Returns the <see cref="DataTreeObject"/> associated with the given <paramref name="node"/>, or <see langword="null"/> if one does not exist.
		/// </summary>
		/// <param name="node">The node used to find the equivalent <see cref="DataTreeObject"/></param>
		public static DataTreeObject GetDataObjectOf(TreeNode node) {
			if (!NodeBindings.ContainsKey(node)) return null;
			return NodeBindings[node];
		}

		/*
		public class DataTreeObjectSelectionChanged {
			/// <summary>
			/// A reference to the equivalent <see cref="DataTreeObject"/> that was clicked.
			/// </summary>
			public DataTreeObject Data { get; }

			/// <summary>
			/// A reference to the <see cref="TreeNode"/> that was clicked.
			/// </summary>
			public TreeNode Node { get; }

			/// <summary>
			/// <see langword="true"/> if the object was selected, <see langword="false"/> if it was deselected.
			/// </summary>
			public bool WasSelected { get; }

			internal DataTreeObjectSelectionChanged(DataTreeObject data, TreeNode node, bool wasSelected) {
				Data = data;
				Node = node;
				WasSelected = wasSelected;
			}
		}
		*/
	}


	/// <summary>
	/// A class that represents a data tree object. It is a basic container with a parent/child hierarchy as well as other applicable data.
	/// </summary>
	public class DataTreeObject : IDisposable {

		/// <summary>
		/// The <see cref="DataTreeObject"/> that contains this instance, or null if this is a root instance.
		/// </summary>
		public DataTreeObject Parent {
			get {
				if (!Locked) return _Parent;
				if (_Parent != null) _Parent.RemoveChild(this);
				return null;
			}
			set {
				if (Locked) throw new ObjectDisposedException("DataTreeObject", "The parent property of DataTreeObject is locked. This object has been destroyed.");
				if (value == _Parent) return;

				if (_Parent != null) {
					_Parent.RemoveChild(this);
				}
				if (value != null) {
					value.AddChild(this);
				}
				_Parent = value;
			}
		}

		/// <summary>
		/// The internal reference of the parent. Do not change this internally, and instead change <see cref="Parent"/>, as it properly updates the object hierarchy.
		/// </summary>
		private DataTreeObject _Parent = null;

		/// <summary>
		/// If true, the <see cref="Parent"/> property of this <see cref="DataTreeObject"/> cannot be changed and will always be null.
		/// </summary>
		private bool Locked = false;

		/// <summary>
		/// All <see cref="DataTreeObject"/>s that are first-level descendants of this object (this does NOT include nested objects).
		/// </summary>
		public IReadOnlyList<DataTreeObject> Children => _Children.AsReadOnly();
		private List<DataTreeObject> _Children = new List<DataTreeObject>();

		/// <summary>
		/// The image key for this <see cref="DataTreeObject"/> which defines the icon displayed to the left of the item in the data tree.<para/>
		/// The default <see cref="SilkImage"/> is <see cref="SilkImage.Generic"/> which represents a generic hierarchy element.<para/>
		/// The "Silk" term comes from the creator of the images, see https://famfamfam.com/
		/// </summary>
		public SilkImage ImageKey { get; set; } = SilkImage.Generic;

		/// <summary>
		/// The text displayed in the data tree for this object.
		/// </summary>
		public string Text { get; set; }

		/// <summary>
		/// The properties of this object. When the associated node is selected, the properties menu will update.<para/>
		/// The method in which this is displayed is via creating the given <see cref="DataTreeObject"/>s in the Properties menu hierarchy, and then adding a child with no icon containing the associated <see langword="string"/>.
		/// </summary>
		public Dictionary<DataTreeObjectProperty, List<DataTreeObjectProperty>> Properties { get; } = new Dictionary<DataTreeObjectProperty, List<DataTreeObjectProperty>>();

		/// <summary>
		/// Iterates through all children of this <see cref="DataTreeObject"/> and sets their <see cref="Parent"/> property to <see langword="null"/>.<para/>
		/// This preserves the hierarchy of the children, so if a child object has its own children, their <see cref="Parent"/>s will remain unchanged.
		/// </summary>
		public void ClearAllChildren() {
			foreach (DataTreeObject child in _Children) {
				child.Parent = null;
			}
		}

		/// <summary>
		/// Sets the <see cref="Parent"/> property of this <see cref="DataTreeObject"/> to <see langword="null"/>, locks the <see cref="Parent"/> property, and then calls <see cref="Dispose"/> on all children.
		/// </summary>
		public void Dispose() {
			Parent = null;
			Locked = true;
			foreach (DataTreeObject child in _Children) {
				child.Dispose();
			}
		}

		/// <summary>
		/// Adds the given child to this <see cref="DataTreeObject"/>'s children.
		/// </summary>
		/// <param name="child"></param>
		protected internal void AddChild(DataTreeObject child) {
			if (!_Children.Contains(child)) {
				_Children.Add(child);
			}
		}

		/// <summary>
		/// Removes the given child from this <see cref="DataTreeObject"/>'s children.
		/// </summary>
		/// <param name="child"></param>
		protected internal void RemoveChild(DataTreeObject child) {
			if (_Children.Contains(child)) {
				_Children.Remove(child);
			}
		}

		/// <summary>
		/// Converts this <see cref="DataTreeObject"/> into a <see cref="TreeNode"/>. This does NOT add any children. If you need to keep the hierarchy, use <see cref="ConvertHierarchyToTreeNodes"/>
		/// </summary>
		/// <returns></returns>
		public TreeNode ToTreeNode() {
			return new TreeNode(Text, (int)ImageKey, (int)ImageKey);
		}

		/// <summary>
		/// Converts this <see cref="DataTreeObject"/> and all children into an identical hierarchy of <see cref="TreeNode"/> objects, and then returns this object as a <see cref="TreeNode"/> with all of the proper children.<para/>
		/// This also connects events to being clicked so that they fire the associated events in <see cref="DataTreeObjectEventMarshaller"/>.
		/// </summary>
		/// <returns></returns>
		public TreeNode ConvertHierarchyToTreeNodes() {
			TreeNode thisNode = ToTreeNode();
			CreateHierarchy(this, thisNode);
			DataTreeObjectEventMarshaller.RegisterTreeNodeBinding(this, thisNode);

			return thisNode;
		}

		/// <summary>
		/// The internal variant of <see cref="ConvertHierarchyToTreeNodes"/> that handles the recursion.
		/// </summary>
		/// <param name="current"></param>
		/// <param name="targetParent"></param>
		protected internal void CreateHierarchy(DataTreeObject current, TreeNode targetParent) {
			foreach (DataTreeObject child in current.Children) {
				TreeNode childAsTreeNode = child.ToTreeNode();
				DataTreeObjectEventMarshaller.RegisterTreeNodeBinding(child, childAsTreeNode);

				if (child.Children.Count > 0) {
					CreateHierarchy(child, childAsTreeNode);
				}
				targetParent.Nodes.Add(childAsTreeNode);
			}
		}

		/// <summary>
		/// An alias method used to add a property with a generic icon to <see cref="Properties"/> (omitting the need to create a <see cref="DataTreeObject"/>)<para/>
		/// If the objec array is a <see cref="DataTreeObjectProperty"/> instance, that instance will be used.
		/// </summary>
		/// <param name="name">The name of the property.</param>
		/// <param name="value">The value displayed under the property.</param>
		/// <param name="propertyNameImage">The image displayed next to the property name.</param>
		/// <param name="propertyValueImages">The image displayed next to each of the values for the property.</param>
		public void AddSimpleProperty(string name, object value, SilkImage propertyNameImage = SilkImage.Value, SilkImage propertyValueImages = SilkImage.Value) => AddSimpleProperty(name, new object[] { value }, propertyNameImage, propertyValueImages);

		/// <summary>
		/// An alias method used to add a property with a generic icon to <see cref="Properties"/> (omitting the need to create a <see cref="DataTreeObject"/>)<para/>
		/// If the object array contains any <see cref="DataTreeObjectProperty"/> instances, those instances will be used (and <paramref name="propertyValueImages"/> will be overridden where applicable).
		/// </summary>
		/// <param name="name">The name of the property.</param>
		/// <param name="values">The values displayed under the property.</param>
		/// <param name="propertyNameImage">The image displayed next to the property name.</param>
		/// <param name="propertyValueImages">The image displayed next to each of the values for the property.</param>
		public void AddSimpleProperty(string name, object[] values, SilkImage propertyNameImage = SilkImage.Value, SilkImage propertyValueImages = SilkImage.Value) {
			DataTreeObjectProperty propName = new DataTreeObjectProperty(name, propertyNameImage);
			List<DataTreeObjectProperty> pValues = new List<DataTreeObjectProperty>();
			foreach (object obj in values) {
				if (obj is DataTreeObjectProperty prop) {
					pValues.Add(prop);
				} else {
					pValues.Add(new DataTreeObjectProperty(obj.ToString(), propertyValueImages));
				}
			}
			Properties[propName] = pValues;
		}

		/// <summary>
		/// Construct a new <see cref="DataTreeObject"/>.
		/// </summary>
		/// <param name="parent">The parent <see cref="DataTreeObject"/> to add this object to.</param>
		public DataTreeObject(DataTreeObject parent = null) {
			Parent = parent;
		}

		/*
		~DataTreeObject() {
			Dispose();
		}
		*/
	}

	/// <summary>
	/// Represents a simpler variant of <see cref="DataTreeObject"/> storing text and an image.
	/// </summary>
	public class DataTreeObjectProperty {
		/// <summary>
		/// The text displayed in the data tree for this object.
		/// </summary>
		public string Text { get; set; }

		/// <summary>
		/// The image key for this <see cref="DataTreeObject"/> which defines the icon displayed to the left of the item in the data tree.<para/>
		/// The default <see cref="SilkImage"/> is <see cref="SilkImage.Generic"/> which represents a generic hierarchy element.<para/>
		/// The "Silk" term comes from the creator of the images, see https://famfamfam.com/
		/// </summary>
		public SilkImage ImageKey { get; set; } = SilkImage.Generic;

		/// <summary>
		/// Converts this <see cref="DataTreeObject"/> into a <see cref="TreeNode"/>.
		/// </summary>
		/// <returns></returns>
		public TreeNode ToTreeNode() {
			return new TreeNode(Text, (int)ImageKey, (int)ImageKey);
		}

		/// <summary>
		/// Construct a new property with empty string and the <see cref="SilkImage.Value"/> image.
		/// </summary>
		/// <param name="text"></param>
		/// <param name="imageKey"></param>
		public DataTreeObjectProperty(string text = "", SilkImage imageKey = SilkImage.Value) {
			Text = text;
			ImageKey = imageKey;
		}
	}
	
	/// <summary>
	/// An enum that represents the available icons in the data tree.<para/>
	/// Note to self: Keep this list updated, and ensure its order is identical to the order in which they are defined by the generated code.
	/// </summary>
	public enum SilkImage {
		Generic,
		Object,
		Scene,
		Sky,
		Model,
		ModelSet,
		Articulated,
		Billboard,
		Static,
		MergedStatic,
		Sound,
		Attachment,
		Derived,
		Conditional,
		CameraShake,
		Generated,
		Schemed,
		SchemedModel,
		Animation,
		Scripted,
		TimedAction,
		Config,
		Shading,
		Reference,
		Array,
		Texture,
		Variant,
		Light,
		Value,
		None
	}
}
