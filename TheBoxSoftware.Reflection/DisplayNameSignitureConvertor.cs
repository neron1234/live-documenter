﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TheBoxSoftware.Reflection {
	using TheBoxSoftware.Reflection.Signitures;

	/// <summary>
	/// A <see cref="SignitureConvertor"/> implementation that creates user
	/// displayable names for types, methods and properties.
	/// </summary>
	public sealed class DisplayNameSignitureConvertor : Signitures.SignitureConvertor {
		private TypeDef type;
		private MethodDef method;
		private PropertyDef property;
		private bool includeNamespace;
		private bool includeParameters = false;
		private bool includeTypeName = true;

		/// <summary>
		/// Initialises a new instance of the DisplayNameSignitureConvertor.
		/// </summary>
		/// <param name="method">The method to obtain a display name for.</param>
		/// <param name="includeNamespace">Should the details of the namespace be included.</param>
		/// <param name="includeParamaters">Should the methods parameters be included.</param>
		public DisplayNameSignitureConvertor(MethodDef method, bool includeNamespace, bool includeParamaters)
			: this() {
			this.type = (TypeDef)method.Type;
			this.method = method;
			this.includeNamespace = includeNamespace;
			this.includeParameters = includeParamaters;
			this.includeTypeName = false;
		}

		/// <summary>
		/// Initialises a new instance of the DisplayNameSignitureConvertor
		/// </summary>
		/// <param name="property">The property to obtain the display name for.</param>
		/// <param name="includeNamespace">Should the details of the namespace be included?</param>
		/// <param name="includeParamaters">Should the parameters for the property be included?</param>
		public DisplayNameSignitureConvertor(PropertyDef property, bool includeNamespace, bool includeParamaters) {
			this.type = (TypeDef)property.Type;
			this.property = property;
			this.method = property.GetMethod;
			this.includeNamespace = includeNamespace;
			this.includeParameters = includeParamaters;
			this.includeTypeName = false;
		}

		/// <summary>
		/// Initialises a new instance of the DisplayNameSignitureConvertor.
		/// </summary>
		/// <param name="type">The type being converted.</param>
		/// <param name="includeNamespace">Should the details of the namespace be included in the display name.</param>
		public DisplayNameSignitureConvertor(TypeDef type, bool includeNamespace) : this() {
			this.type = type;
			this.includeNamespace = includeNamespace;
		}

		/// <summary>
		/// Initialises a new instance of the DisplayNameSignitureConvertor.
		/// </summary>
		private DisplayNameSignitureConvertor() {
			// Set the generic element tags
			this.GenericEnd = ">";
			this.GenericStart = "<";
			this.ByRef = "ref ";
			this.ByRefAtFront = true;
			this.ParameterSeperater = ", ";
		}

		/// <summary>
		/// Implementation of the convert method, that is used to create a display
		/// version of the signiture this convertor has been instantiated with.
		/// </summary>
		/// <returns>The fully converted signiture as a string.</returns>
		public string Convert() {
			StringBuilder converted = new StringBuilder();

			// Convert the type portion
			if (this.includeTypeName) {
				this.GetTypeName(converted, this.type);
				if (this.type.IsGeneric) {
					converted.Append(this.GenericStart);
					bool first = true;
					foreach (GenericTypeRef type in this.type.GetGenericTypes()) {
						if (first) {
							first = false;
						}
						else {
							converted.Append(", ");
						}
						converted.Append(type.Name);
					}
					converted.Append(this.GenericEnd);
				}
			}

			// Fix to make sure properties are displayed correctly when
			// they have parameters

			// Convert the method portion
			if (method != null) {
				if (method.IsConstructor && property == null) {
					this.GetTypeName(converted, this.type);
				}
				else if (method.IsOperator && property == null) {
					if (method.IsConversionOperator) {
						converted.Append(method.Name.Substring(3));
						converted.Append("(");
						TypeRef convertToRef = method.Signiture.GetReturnTypeToken().ResolveType(method.Assembly, method);
						converted.Append(convertToRef.Name);
						converted.Append(" to ");
						converted.Append(method.Type.GetDisplayName(false));
						converted.Append(")");
					}
					else {
						converted.Append(method.Name.Substring(3));
					}
				}
				else if (property == null) {
					converted.Append(method.Name);
				}
				else {
					converted.Append(property.Name);
				}
				if (method.IsGeneric) {
					converted.Append(this.GenericStart);
					bool first = true;
					foreach (GenericTypeRef type in method.GetGenericTypes()) {
						if (first) {
							first = false;
						}
						else {
							converted.Append(", ");
						}
						converted.Append(type.Name);
					}
					converted.Append(this.GenericEnd);
				}

				if (this.includeParameters && !method.IsConversionOperator) {
					string parameters = this.Convert(method);
					if (string.IsNullOrEmpty(parameters) && property == null) {
						parameters = "()";
					}
					converted.Append(parameters);
				}
			}
			return converted.ToString();
		}

		/// <summary>
		/// Obtains the type name for the specified <see cref="TypeRef"/>.
		/// </summary>
		/// <param name="sb">The current display name for the signiture to append the details to.</param>
		/// <param name="type">The type to obtain the display name for.</param>
		protected override void GetTypeName(StringBuilder sb, TypeRef type) {
			string typeName = type.GetFullyQualifiedName();
			if (!this.includeNamespace) {
				typeName = type.Name;
			}
			else {
				typeName = type.GetFullyQualifiedName();
			}

			if (type.IsGeneric) {
				int genIdIndex = typeName.IndexOf('`');
				if (genIdIndex != -1) {
					typeName = typeName.Substring(0, genIdIndex);
				}
			}

			sb.Append(typeName);
		}

		/// <summary>
		/// Converts a generic variable for display.
		/// </summary>
		/// <param name="sb">The current display name for the signiture to append details to.</param>
		/// <param name="sequence">The sequence number of the current generic variable.</param>
		/// <param name="parameter">The parameter definition information.</param>
		protected override void ConvertMVar(StringBuilder sb, int sequence, ParamDef parameter) {
			// Type Generic Parameter
			GenericTypeRef foundGenericType = null;
			foreach (GenericTypeRef current in this.method.GenericTypes) {
				if (current.Sequence == sequence) {
					foundGenericType = current;
					break;
				}
			}
			if (foundGenericType != null) {
				sb.Append(foundGenericType.Name);
			}
		}

		/// <summary>
		/// Converts a generic variable for display.
		/// </summary>
		/// <param name="sb">The current display name for the signiture to append details to.</param>
		/// <param name="sequence">The sequence number of the current generic variable</param>
		/// <param name="parameter">The parameter definition information.</param>
		protected override void ConvertVar(StringBuilder sb, int sequence, ParamDef parameter) {
			// Type Generic Parameter
			GenericTypeRef foundGenericType = null;
			foreach (GenericTypeRef current in this.type.GenericTypes) {
				if (current.Sequence == sequence) {
					foundGenericType = current;
					break;
				}
			}
			if (foundGenericType != null) {
				sb.Append(foundGenericType.Name);
			}
		}

		/// <summary>
		/// Overridden convertor for arrays. Converts the <see cref="ArrayShapeSignitureToken"/>
		/// to its correct display name equivelant.
		/// </summary>
		/// <param name="sb">The string being constructed containing the display name.</param>
		/// <param name="resolvedType">The type the parameter has been resolved to</param>
		/// <param name="shape">The signiture token detailing the shape of the array.</param>
		protected override void ConvertArray(StringBuilder sb, TypeRef resolvedType, ArrayShapeSignitureToken shape) {
			this.GetTypeName(sb, resolvedType);
			sb.Append("[");
			for (int i = 0; i < shape.Rank; i++) {
				if (i != 0 && i != shape.Rank) {
					sb.Append(",");
				}
				bool hasLoBound = i < shape.NumLoBounds;
				bool hasSize = i < shape.NumSizes;
				if (hasLoBound) {
					sb.Append(shape.LoBounds[i]);
				}
				if (hasSize) {
					sb.Append(shape.Sizes[i]);
				}
			}
			sb.Append("]");
		}
	}
}
