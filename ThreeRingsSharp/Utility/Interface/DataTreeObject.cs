using System;
using System.Collections.Generic;
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


	}


	/// <summary>
	/// A class that represents a data tree object. It is a basic container with a parent/child hierarchy as well as other applicable data.
	/// </summary>
	public class DataTreeObject : IDisposable {

		/// <summary>
		/// The <see cref="DataTreeObject"/> that contains this instance, or null if this is a root instance.<para/>
		/// Setting this will update the children of the applicable objects (remove this from the children of the old parent (if applicable), add this to the children of the new parent (if applicable)) automatically.
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

		/// <summary>
		/// An internal reference to the children of this <see cref="DataTreeObject"/>.
		/// </summary>
		private readonly List<DataTreeObject> _Children = new List<DataTreeObject>();

		/// <summary>
		/// The image key for this <see cref="DataTreeObject"/> which defines the icon displayed to the left of the item in the data tree.<para/>
		/// The default <see cref="SilkImage"/> is <see cref="SilkImage.Generic"/> which represents a generic hierarchy element.
		/// </summary>
		public SilkImage ImageKey { get; set; } = SilkImage.Generic;

		/// <summary>
		/// The text displayed in the data tree for this object.
		/// </summary>
		public string Text { get; set; }

		/// <summary>
		/// Internal compatibility for casts between this and <see cref="DataTreeObjectProperty"/>
		/// </summary>
		internal bool DisplaySingleChildInline { get; set; } = true;

		/// <summary>
		/// If true, this <see cref="DataTreeObject"/> was cast from a <see cref="DataTreeObjectProperty"/>.
		/// </summary>
		public bool CreatedFromProperty { get; internal set; } = false;

		/// <summary>
		/// The properties of this object. When the associated node is selected, the properties menu will update.<para/>
		/// The method in which this is displayed is via creating the given <see cref="DataTreeObject"/>s in the Properties menu hierarchy, and then adding a child with no icon containing the associated <see langword="string"/>.
		/// </summary>
		public Dictionary<DataTreeObjectProperty, List<DataTreeObject>> Properties { get; } = new Dictionary<DataTreeObjectProperty, List<DataTreeObject>>();

		/// <summary>
		/// Additional data that might be stored in this property.
		/// </summary>
		public Dictionary<string, object> ExtraData { get; internal set; } = new Dictionary<string, object>();

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
		/// If the object array is a <see cref="DataTreeObjectProperty"/> instance, that instance will be used.
		/// </summary>
		/// <param name="name">The name of the property.</param>
		/// <param name="value">The value displayed under the property.</param>
		/// <param name="propertyNameImage">The image displayed next to the property name.</param>
		/// <param name="propertyValueImages">The image displayed next to each of the values for the property.</param>
		/// <param name="displaySinglePropertiesInline">If true, properties with single values will be displayed in the same element containing the property name (<c>Name: Value</c>) instead of as a child element (<c>Name</c>, with a child of <c>Value</c>).</param>
		public DataTreeObjectProperty AddSimpleProperty(string name, object value, SilkImage propertyNameImage = SilkImage.Value, SilkImage propertyValueImages = SilkImage.Value, bool displaySinglePropertiesInline = true) => AddSimpleProperty(name, new object[] { value }, propertyNameImage, propertyValueImages, displaySinglePropertiesInline);

		/// <summary>
		/// An alias method used to add a property with a generic icon to <see cref="Properties"/> (omitting the need to create a <see cref="DataTreeObject"/>)<para/>
		/// If the object array contains any <see cref="DataTreeObjectProperty"/> instances, those instances will be used (and <paramref name="propertyValueImages"/> will be overridden where applicable).
		/// </summary>
		/// <param name="name">The name of the property.</param>
		/// <param name="values">The values displayed under the property.</param>
		/// <param name="propertyNameImage">The image displayed next to the property name.</param>
		/// <param name="propertyValueImages">The image displayed next to each of the values for the property.</param>
		/// <param name="displaySinglePropertiesInline">If true, properties with single values will be displayed in the same element containing the property name (<c>Name: Value</c>) instead of as a child element (<c>Name</c>, with a child of <c>Value</c>).</param>
		public DataTreeObjectProperty AddSimpleProperty(string name, object[] values, SilkImage propertyNameImage = SilkImage.Value, SilkImage propertyValueImages = SilkImage.Value, bool displaySinglePropertiesInline = true) {
			DataTreeObjectProperty propName = new DataTreeObjectProperty(name, propertyNameImage, displaySinglePropertiesInline);
			List<DataTreeObject> pValues = new List<DataTreeObject>();
			foreach (object obj in values) {
				if (obj is DataTreeObject objInstance) {
					pValues.Add(objInstance);
				} else if (obj is DataTreeObjectProperty prop) {
					pValues.Add(prop);
				} else {
					if (obj is null) {
						pValues.Add(new DataTreeObjectProperty("null", propertyValueImages));
					} else {
						pValues.Add(new DataTreeObjectProperty(obj.ToString(), propertyValueImages));
					}
				}
			}
			Properties[propName] = pValues;
			return propName;
		}

		/// <summary>
		/// Attempts to locate a property with the given name, and then updates its values. This will do nothing if the property cannot be found.<para/>
		/// Any <see langword="null"/> parameters will retain their existing values.
		/// </summary>
		/// <param name="name">The name to search for.</param>
		/// <param name="newName">If <see langword="null"/>, the name will remain unchanged. If defined, the name of the property will be changed to this.</param>
		/// <param name="value">The new value to set the property to.</param>
		/// <param name="propertyNameImage">The image displayed next to the property name.</param>
		/// <param name="propertyValueImages">The image displayed next to each of the values for the property.</param>
		/// <param name="displaySinglePropertiesInline">If true, properties with single values will be displayed in the same element containing the property name (<c>Name: Value</c>) instead of as a child element (<c>Name</c>, with a child of <c>Value</c>).</param>
		public void EditSimpleProperty(string name, string newName = null, object value = null, SilkImage? propertyNameImage = null, SilkImage? propertyValueImages = null, bool? displaySinglePropertiesInline = null) => EditSimpleProperty(name, newName, new object[] { value }, propertyNameImage, propertyValueImages, displaySinglePropertiesInline);

		/// <summary>
		/// Updates the property with the given data.<para/>
		/// Any <see langword="null"/> parameters will retain their existing values.
		/// </summary>
		/// <param name="key">The property to edit.</param>
		/// <param name="newName">If <see langword="null"/>, the name will remain unchanged. If defined, the name of the property will be changed to this.</param>
		/// <param name="value">The new value to set the property to.</param>
		/// <param name="propertyNameImage">The image displayed next to the property name.</param>
		/// <param name="propertyValueImages">The image displayed next to each of the values for the property.</param>
		/// <param name="displaySinglePropertiesInline">If true, properties with single values will be displayed in the same element containing the property name (<c>Name: Value</c>) instead of as a child element (<c>Name</c>, with a child of <c>Value</c>).</param>
		public void EditSimpleProperty(DataTreeObjectProperty key, string newName = null, object value = null, SilkImage? propertyNameImage = null, SilkImage? propertyValueImages = null, bool? displaySinglePropertiesInline = null) => EditSimpleProperty(key, newName, new object[] { value }, propertyNameImage, propertyValueImages, displaySinglePropertiesInline);

		/// <summary>
		/// Attempts to locate a property with the given name, and then updates its values. This will do nothing if the property cannot be found.<para/>
		/// Any <see langword="null"/> parameters will retain their existing values.
		/// </summary>
		/// <param name="name">The name to search for.</param>
		/// <param name="newName">If defined, the name of the property will be changed to this.</param>
		/// <param name="values">The new values to set the property to.</param>
		/// <param name="propertyNameImage">The image displayed next to the property name.</param>
		/// <param name="propertyValueImages">The image displayed next to each of the values for the property.</param>
		/// <param name="displaySinglePropertiesInline">If true, properties with single values will be displayed in the same element containing the property name (<c>Name: Value</c>) instead of as a child element (<c>Name</c>, with a child of <c>Value</c>).</param>
		public void EditSimpleProperty(string name, string newName = null, object[] values = null, SilkImage? propertyNameImage = null, SilkImage? propertyValueImages = null, bool? displaySinglePropertiesInline = null) {
			foreach (DataTreeObjectProperty propContainer in Properties.Keys) {
				if (propContainer.Text == name) {
					// This is the one that needs to be edited.
					EditSimpleProperty(propContainer, newName, values, propertyNameImage, propertyValueImages, displaySinglePropertiesInline);
					return;
				}
			}
		}

		/// <summary>
		/// Updates the property with the given data. If the property does not exist, it will be added.<para/>
		/// Any <see langword="null"/> parameters will retain their existing values.
		/// </summary>
		/// <param name="key">The property to edit.</param>
		/// <param name="newName">If defined, the name of the property will be changed to this.</param>
		/// <param name="values">The new values to set the property to.</param>
		/// <param name="propertyNameImage">The image displayed next to the property name.</param>
		/// <param name="propertyValueImages">The image displayed next to each of the values for the property.</param>
		/// <param name="displaySinglePropertiesInline">If true, properties with single values will be displayed in the same element containing the property name (<c>Name: Value</c>) instead of as a child element (<c>Name</c>, with a child of <c>Value</c>).</param>
		public void EditSimpleProperty(DataTreeObjectProperty key, string newName = null, object[] values = null, SilkImage? propertyNameImage = null, SilkImage? propertyValueImages = null, bool? displaySinglePropertiesInline = null) {
			key.Text = newName ?? key.Text;
			List<DataTreeObject> pValues = new List<DataTreeObject>();
			if (values != null) {
				foreach (object obj in values) {
					if (obj == null) continue;
					if (obj is DataTreeObject objInstance) {
						pValues.Add(objInstance);
					} else if (obj is DataTreeObjectProperty prop) {
						pValues.Add(prop);
					} else {
						pValues.Add(new DataTreeObjectProperty(obj.ToString(), propertyValueImages.GetValueOrDefault(SilkImage.Value)));
					}
				}
			}
			key.ImageKey = propertyNameImage ?? key.ImageKey;
			key.DisplaySingleChildInline = displaySinglePropertiesInline ?? key.DisplaySingleChildInline;
			Properties[key] = pValues;
		}

		/// <summary>
		/// Locates a given <see cref="DataTreeObjectProperty"/> in <see cref="Properties"/> whose key is the given <paramref name="name"/>.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public DataTreeObjectProperty FindSimpleProperty(string name) {
			foreach (DataTreeObjectProperty propContainer in Properties.Keys) {
				if (propContainer.Text == name) {
					return propContainer;
				}
			}
			return null;
		}

		/// <summary>
		/// Deletes a given <see cref="DataTreeObjectProperty"/> from <see cref="Properties"/>.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public void DeleteSimpleProperty(string name) {
			var keys = Properties.Keys;
			foreach (DataTreeObjectProperty propContainer in keys) {
				if (propContainer.Text == name) {
					Properties.Remove(propContainer);
					return;
				}
			}
		}

		/// <summary>
		/// Construct a new <see cref="DataTreeObject"/>.
		/// </summary>
		/// <param name="parent">The parent <see cref="DataTreeObject"/> to add this object to.</param>
		public DataTreeObject(DataTreeObject parent = null) {
			Parent = parent;
		}

		public static explicit operator DataTreeObjectProperty(DataTreeObject obj) {
			if (obj is null) return null;
			return new DataTreeObjectProperty(obj.Text, obj.ImageKey, obj.DisplaySingleChildInline) {
				CreatedFromFullObject = !obj.CreatedFromProperty,
				ExtraData = obj.ExtraData
			};
		}
	}

	/// <summary>
	/// Represents a simpler variant of <see cref="DataTreeObject"/> storing text and an image. It cannot have children.
	/// </summary>
	public class DataTreeObjectProperty {

		/// <summary>
		/// The text displayed in the data tree for this object.
		/// </summary>
		public string Text { get; set; }

		/// <summary>
		/// If <see langword="true"/>, properties with single values will be displayed in the same element containing the property name (<c>Name: Value</c>) instead of as a child element (<c>Name</c>, with a child of <c>Value</c>).<para/>
		/// This object must be a key in a <see cref="DataTreeObject.Properties"/> for this to do anything. When the GUI system creates the properties menu, if this property object has one associated child value, it will display inline if this is <see langword="true"/>.
		/// </summary>
		public bool DisplaySingleChildInline { get; set; }

		/// <summary>
		/// The image key for this <see cref="DataTreeObject"/> which defines the icon displayed to the left of the item in the data tree.<para/>
		/// The default <see cref="SilkImage"/> is <see cref="SilkImage.Generic"/> which represents a generic hierarchy element.<para/>
		/// The "Silk" term comes from the creator of the images, see https://famfamfam.com/
		/// </summary>
		public SilkImage ImageKey { get; set; } = SilkImage.Generic;

		/// <summary>
		/// If true, this <see cref="DataTreeObjectProperty"/> was cast from a <see cref="DataTreeObject"/>.
		/// </summary>
		public bool CreatedFromFullObject { get; internal set; } = false;

		/// <summary>
		/// Converts this <see cref="DataTreeObject"/> into a <see cref="TreeNode"/>.
		/// </summary>
		/// <returns></returns>
		public TreeNode ToTreeNode() {
			return new TreeNode(Text, (int)ImageKey, (int)ImageKey);
		}

		/// <summary>
		/// Additional data that might be stored in this property.
		/// </summary>
		public Dictionary<string, object> ExtraData { get; internal set; } = new Dictionary<string, object>();

		/// <summary>
		/// Construct a new property with empty string and the <see cref="SilkImage.Value"/> image.
		/// </summary>
		/// <param name="text">The text to display in this property.</param>
		/// <param name="imageKey">The image to display to the left of the text.</param>
		/// <param name="displaySinglePropertiesInline">If true, properties with single values will be displayed in the same element containing the property name (<c>Name: Value</c>) instead of as a child element (<c>Name</c>, with a child of <c>Value</c>). In the case of this constructor, this object must be a key in a <see cref="DataTreeObject.Properties"/>. When the GUI system creates the properties menu, if this property object has one associated child value, it will display inline as mentioend prior.</param>
		public DataTreeObjectProperty(string text = "", SilkImage imageKey = SilkImage.Value, bool displaySinglePropertiesInline = true) {
			Text = text;
			ImageKey = imageKey;
			DisplaySingleChildInline = displaySinglePropertiesInline;
		}

		public static implicit operator DataTreeObject(DataTreeObjectProperty prop) {
			if (prop is null) return null;
			return new DataTreeObject() {
				Text = prop.Text,
				ImageKey = prop.ImageKey,
				DisplaySingleChildInline = prop.DisplaySingleChildInline,
				CreatedFromProperty = !prop.CreatedFromFullObject,
				ExtraData = prop.ExtraData
			};
		}
	}
}
