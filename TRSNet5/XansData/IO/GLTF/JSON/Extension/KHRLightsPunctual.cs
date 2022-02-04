using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThreeRingsSharp.XansData.IO.GLTF.JSON.Extension {

	/// <summary>
	/// <strong>This is an extension object that is not natively supported by glTF 2.0 Specification.</strong><para/>
	/// Defined as <see href="https://github.com/KhronosGroup/glTF/blob/main/extensions/2.0/Khronos/KHR_lights_punctual/README.md">KHR_lights_punctual</see>, this extension provides
	/// a means of creating various dynamic light types in the scene.
	/// </summary>
	[Obsolete("Lights are not ready")]
	public class KHRLightsPunctual : GLTFObject, IGLTFExtension {

		public static KHRLightsPunctual FromLights(IEnumerable<Light> lights) {
			KHRLightsPunctual lightsExtensionDefinition = new KHRLightsPunctual();
			int index = 0;
			foreach (Light light in lights) {
				KHRLight khrLight = KHRLight.FromLight(light);
				khrLight.ThisIndex = index;
				lightsExtensionDefinition.Lights.Add(khrLight);
				index++;
			}
			return lightsExtensionDefinition;
		}

		[JsonIgnore]
		public string ExtensionName { get; } = "KHR_lights_punctual";

		[JsonProperty("lights")]
		public List<KHRLight> Lights { get; } = new List<KHRLight>();

		public class KHRLight : IEquatable<KHRLight> {

			/// <summary>
			/// The type used for point lights. These emit in a 360 degree sphere, or, in all directions.
			/// </summary>
			public const string POINT = "point";

			/// <summary>
			/// The type used for spot lights. These emit in a specific direction.
			/// </summary>
			public const string SPOT = "spot";

			/// <summary>
			/// The type used for directional lights. For Blender users, this is an area light.
			/// </summary>
			public const string DIRECTIONAL = "directional";

			public int ThisIndex { get; internal set; }

			/// <summary>
			/// Creates a new <see cref="KHRLight"/> from a TRS <see cref="Light"/> instance.
			/// </summary>
			/// <param name="light"></param>
			/// <returns></returns>
			public static KHRLight FromLight(Light light) {
				return new KHRLight {

				};
			}

			/// <summary>
			/// The name of this light. Optional.
			/// </summary>
			[JsonProperty("name")]
			public string Name { get; set; } = string.Empty;

			/// <summary>
			/// The color of this light in f32 R,G,B format. This is in linear space. Optional, default is [1, 1, 1] (white).
			/// </summary>
			[JsonProperty("color")]
			public float[] Color { get; } = new float[] { 1f, 1f, 1f };

			/// <summary>
			/// The intensity of the light. Default is 1. For point and spot lights, this is in candela (lm/sr), but directional lights use lux (lm/m²)
			/// </summary>
			[JsonProperty("intensity")]
			public float Intensity { get; set; } = 1.0f;

			/// <summary>
			/// A hint defining the distance cutoff at which the light's intensity may be considered to have reached zero. Supported only on <see cref="SPOT"/> and <see cref="POINT"/>.
			/// This is optional, and its default value is <see cref="float.PositiveInfinity"/>.
			/// </summary>
			[JsonProperty("range")]
			public float Range { get; set; } = float.PositiveInfinity;

			/// <summary>
			/// The type of light this is. Required, defaults to <see cref="POINT"/>.
			/// </summary>
			[JsonIgnore]
			public string Type {
				get => _type;
				set {
					if (value == POINT || value == SPOT || value == DIRECTIONAL) {
						_type = value;
					}
					throw new ArgumentOutOfRangeException(nameof(value), $"Type can only be one of \"{POINT}\", \"{SPOT}\", or \"{DIRECTIONAL}\". Consider using the constants provided by {nameof(KHRLightsPunctual)} to prevent unwanted input.");
				}
			}
			[JsonProperty("type")]
			private string _type = POINT;

			/// <summary>
			/// The settings for the light. This is strictly used when <see cref="Type"/> is <see cref="SPOT"/>. It is ignored in all other cases.
			/// </summary>
			[JsonProperty("spot")]
			public SpotSettings SpotlightSettings { get; } = new SpotSettings();

			public override int GetHashCode() {
				return HashCode.Combine(Color, Intensity, Range, Type);
			}

			public override bool Equals(object? obj) {
				if (obj is KHRLight light) return Equals(light);
				return false;
			}

			public bool Equals(KHRLight? other) {
				if (other is null) return false;
				if (ReferenceEquals(this, other)) return true;
				bool baseEquals = Color.SequenceEqual(other.Color) &&
					Intensity == other.Intensity &&
					Range == other.Range &&
					Name == other.Name &&
					Type == other.Type;
				if (Type == SPOT && baseEquals) {
					baseEquals = baseEquals && (SpotlightSettings == other.SpotlightSettings);
				}
				return baseEquals;
			}

			public static bool operator ==(KHRLight? left, KHRLight? right) => left?.Equals(right) ?? false;

			public static bool operator !=(KHRLight? left, KHRLight? right) => !(left == right);

			#region Newtonsoft Serialization Stuffs

			public bool ShouldSerializeSpotlightSettings() {
				if (Type == SPOT) {
					// verify the settings
					if (SpotlightSettings.InnerConeAngle >= 0 && SpotlightSettings.InnerConeAngle < SpotlightSettings.OuterConeAngle && SpotlightSettings.OuterConeAngle <= MathF.PI / 2) {
						return true;
					}
					throw new InvalidOperationException($"Cannot serialize \"{Name}\": Its spotlight settings are malformed. Either its inner cone angle is less than zero, its inner cone angle is greater than or equal to its outer cone angle, or its outer cone angle is larger than pi/2.");
				}
				return false;
			}

			#endregion

			/// <summary>
			/// A class representing spotlight settings.
			/// </summary>
			public sealed class SpotSettings : IEquatable<SpotSettings> {

				/// <summary>
				/// Angle, in radians, from the center of the spotlight where its falloff begins. Must be greater than or equal to 0 and less than <see cref="OuterConeAngle"/>.
				/// This property is validated when serializing. Its default value is 0.
				/// </summary>
				[JsonProperty("innerConeAngle")]
				public float InnerConeAngle { get; set; } = 0f;

				/// <summary>
				/// Angle, in radians, from the center of the spotlight where its falloff ends. This must be greater than <see cref="InnerConeAngle"/> and less than or equal to pi/2.
				/// This property is validated when serializing. Its default value is pi/4
				/// </summary>
				[JsonProperty("outerConeAngle")]
				public float OuterConeAngle { get; set; } = MathF.PI / 4;

				public static bool operator ==(SpotSettings? left, SpotSettings? right) => left?.Equals(right) ?? false;

				public static bool operator !=(SpotSettings? left, SpotSettings? right) => !(left == right);

				public override int GetHashCode() {
					return HashCode.Combine(InnerConeAngle, OuterConeAngle);
				}

				public override bool Equals(object? obj) {
					if (obj is SpotSettings other) return Equals(other);
					return false;
				}

				public bool Equals(SpotSettings? other) {
					if (other is null) return false;
					if (ReferenceEquals(other, this)) return true;
					return other.InnerConeAngle == InnerConeAngle && other.OuterConeAngle == OuterConeAngle;
				}
			}
		}

		public sealed class KHRLightReference {

			/// <summary>
			/// A reference to the light that created this <see cref="KHRLightReference"/>
			/// </summary>
			[JsonIgnore]
			public KHRLight LightObject { get; }

			/// <summary>
			/// The index of the light in the registry. This is defined by the <see cref="KHRLightsPunctual"/> instance that created it.
			/// </summary>
			public int Light => LightObject.ThisIndex;

			public KHRLightReference(KHRLight light) {
				LightObject = light;
			}

		}
	}
}
