﻿#region MIT.
// 
// Gorgon.
// Copyright (C) 2011 Michael Winsor
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: Monday, November 28, 2011 8:28:24 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using D3D = SharpDX.Direct3D11;
using GorgonLibrary.Diagnostics;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Operations applied to stencil buffers.
	/// </summary>
	public enum StencilOperations
	{
		/// <summary>
		/// Keep existing stencil data.
		/// </summary>
		Keep = 1,
		/// <summary>
		/// Set the stencil data to 0.
		/// </summary>
		Zero = 2,
		/// <summary>
		/// Set the stencil data to a reference value defined in the depth/stencil state object.
		/// </summary>
		Replace = 3,
		/// <summary>
		/// Increment the stencil value by 1 and clamp the result.
		/// </summary>
		IncrementClamp = 4,
		/// <summary>
		/// Decrement the stencil value by 1 and clamp the result.
		/// </summary>
		DecrementClamp = 5,
		/// <summary>
		/// Invert the stencil value.
		/// </summary>
		Invert = 6,
		/// <summary>
		/// Increment the stencil value by 1 and wrap if necessary.
		/// </summary>
		Increment = 7,
		/// <summary>
		/// Decrement the stencil value by 1 and wrap if necessary.
		/// </summary>
		Decrement = 8
	}

	/// <summary>
	/// States for the depth/stencil.
	/// </summary>
	public struct GorgonDepthStencilStates
	: IEquatable<GorgonDepthStencilStates>
	{
		#region Value Types.
		/// <summary>
		/// Operations to perform on the depth/stencil buffer.
		/// </summary>
		public struct DepthStencilOperations
			: IEquatable<DepthStencilOperations>
		{
			#region Variables.
			/// <summary>
			/// The comparison operator for the stencil testing.
			/// </summary>
			/// <remarks>The default value is Always.</remarks>
			public ComparisonOperators ComparisonOperator;

			/// <summary>
			/// The operation to perform when the test fails.
			/// </summary>
			/// <remarks>The default value is Keep.</remarks>
			public StencilOperations FailOperation;

			/// <summary>
			/// The operation to perform when the depth test fails.
			/// </summary>
			/// <remarks>The default value is Keep.</remarks>
			public StencilOperations DepthFailOperation;

			/// <summary>
			/// The operation to perform when the test succeeds.
			/// </summary>
			/// <remarks>The default value is Keep.</remarks>
			public StencilOperations PassOperation;
			#endregion

			#region Methods.
			/// <summary>
			/// Determines whether the specified objects are equal.
			/// </summary>
			/// <param name="x">The first depth stencil operation to compare.</param>
			/// <param name="y">The second depth stencil operation to compare.</param>
			/// <returns>
			/// true if the specified objects are equal; otherwise, false.
			/// </returns>
			public static bool Equals(ref DepthStencilOperations x, ref DepthStencilOperations y)
			{
				return ((x.ComparisonOperator == y.ComparisonOperator) && (x.FailOperation == y.FailOperation) && (x.DepthFailOperation == y.DepthFailOperation) && (x.PassOperation == y.PassOperation));
			}

			/// <summary>
			/// Returns a hash code for this instance.
			/// </summary>
			/// <returns>
			/// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
			/// </returns>
			public override int GetHashCode()
			{
				unchecked
				{
					return 281.GenerateHash(ComparisonOperator).
								GenerateHash(FailOperation).
								GenerateHash(DepthFailOperation).
								GenerateHash(PassOperation);
				}
			}

			/// <summary>
			/// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
			/// </summary>
			/// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
			/// <returns>
			///   <c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
			/// </returns>
			public override bool Equals(object obj)
			{
				if (obj is DepthStencilOperations)
					return this.Equals((DepthStencilOperations)obj);

				return base.Equals(obj);
			}

			/// <summary>
			/// Implements the operator ==.
			/// </summary>
			/// <param name="left">The left.</param>
			/// <param name="right">The right.</param>
			/// <returns>
			/// The result of the operator.
			/// </returns>
			public static bool operator ==(DepthStencilOperations left, DepthStencilOperations right)
			{
				return DepthStencilOperations.Equals(ref left, ref right);
			}

			/// <summary>
			/// Implements the operator !=.
			/// </summary>
			/// <param name="left">The left.</param>
			/// <param name="right">The right.</param>
			/// <returns>
			/// The result of the operator.
			/// </returns>
			public static bool operator !=(DepthStencilOperations left, DepthStencilOperations right)
			{
				return !DepthStencilOperations.Equals(ref left, ref right);
			}
			#endregion

			#region IEquatable<DepthStencilOperations> Members
			/// <summary>
			/// Indicates whether the current object is equal to another object of the same type.
			/// </summary>
			/// <param name="other">An object to compare with this object.</param>
			/// <returns>
			/// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
			/// </returns>
			public bool Equals(DepthStencilOperations other)
			{
				return ((ComparisonOperator == other.ComparisonOperator) && (FailOperation == other.FailOperation) && (DepthFailOperation == other.DepthFailOperation) && (PassOperation == other.PassOperation));
			}
			#endregion
		}
		#endregion

		#region Variables.
		/// <summary>
		/// Default depth/stencil states.
		/// </summary>
		public static readonly GorgonDepthStencilStates DefaultStates = new GorgonDepthStencilStates()
		{
			IsDepthEnabled = true,
			IsDepthWriteEnabled = true,
			IsStencilEnabled = false,
			StencilReadMask = 0xff,
			StencilWriteMask = 0xff,
			DepthComparison = ComparisonOperators.Less,
			StencilFrontFace = new DepthStencilOperations()
			{
				ComparisonOperator = ComparisonOperators.Always,
				DepthFailOperation = StencilOperations.Keep,
				FailOperation = StencilOperations.Keep,
				PassOperation = StencilOperations.Keep
			},
			StencilBackFace = new DepthStencilOperations()
			{
				ComparisonOperator = ComparisonOperators.Always,
				DepthFailOperation = StencilOperations.Keep,
				FailOperation = StencilOperations.Keep,
				PassOperation = StencilOperations.Keep
			}
		};

		/// <summary>
		/// Operations to perform on the front face in a stencil test.
		/// </summary>
		public DepthStencilOperations StencilFrontFace;

		/// <summary>
		/// Operations to perform on the back face in a stencil test.
		/// </summary>
		public DepthStencilOperations StencilBackFace;

		/// <summary>
		/// Is the depth buffer enabled or not.
		/// </summary>
		/// <remarks>The default value is TRUE.</remarks>
		public bool IsDepthEnabled;

		/// <summary>
		/// Is the stencil buffer enabled or not.
		/// </summary>
		/// <remarks>The default value is FALSE.</remarks>
		public bool IsStencilEnabled;

		/// <summary>
		/// Is depth writing enabled or not.
		/// </summary>
		/// <remarks>The default value is TRUE.</remarks>
		public bool IsDepthWriteEnabled;

		/// <summary>
		/// Comparison operator for the depth buffer test.
		/// </summary>
		/// <remarks>The default value is Less</remarks>
		public ComparisonOperators DepthComparison;

		/// <summary>
		/// The mask used to read from the stencil buffer.
		/// </summary>
		/// <remarks>The default value is 0xFF.</remarks>
		public byte StencilReadMask;

		/// <summary>
		/// The mask used to write to the stencil buffer.
		/// </summary>
		/// <remarks>The default value is 0xFF.</remarks>
		public byte StencilWriteMask;
		#endregion

		#region Methods.
		/// <summary>
		/// Determines whether the specified objects are equal.
		/// </summary>
		/// <param name="x">The first depth stencil state to compare.</param>
		/// <param name="y">The second depth stencil state to compare.</param>
		/// <returns>
		/// true if the specified objects are equal; otherwise, false.
		/// </returns>
		public static bool Equals(ref GorgonDepthStencilStates x, ref GorgonDepthStencilStates y)
		{
			return ((DepthStencilOperations.Equals(ref x.StencilFrontFace, ref y.StencilFrontFace)) && (DepthStencilOperations.Equals(ref x.StencilBackFace, ref y.StencilBackFace)) &&
					(x.DepthComparison == y.DepthComparison) && (x.IsDepthEnabled == y.IsDepthEnabled) && (x.IsDepthWriteEnabled == y.IsDepthWriteEnabled) &&
					(x.IsStencilEnabled == y.IsStencilEnabled) && (x.StencilReadMask == y.StencilReadMask) && (x.StencilWriteMask == y.StencilWriteMask));
		}

		/// <summary>
		/// Returns a hash code for this instance.
		/// </summary>
		/// <returns>
		/// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
		/// </returns>
		public override int GetHashCode()
		{
			unchecked
			{
				return 281.GenerateHash(StencilBackFace).
						GenerateHash(StencilBackFace).
						GenerateHash(IsDepthEnabled).
						GenerateHash(IsDepthWriteEnabled).
						GenerateHash(IsStencilEnabled).
						GenerateHash(DepthComparison).
						GenerateHash(StencilReadMask).
						GenerateHash(StencilWriteMask);
			}
		}

		/// <summary>
		/// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
		/// </summary>
		/// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
		/// <returns>
		///   <c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
		/// </returns>
		public override bool Equals(object obj)
		{
			if (obj is GorgonDepthStencilStates)
				return Equals((GorgonDepthStencilStates)obj);

			return base.Equals(obj);
		}

		/// <summary>
		/// Implements the operator ==.
		/// </summary>
		/// <param name="left">The left.</param>
		/// <param name="right">The right.</param>
		/// <returns>
		/// The result of the operator.
		/// </returns>
		public static bool operator ==(GorgonDepthStencilStates left, GorgonDepthStencilStates right)
		{
			return GorgonDepthStencilStates.Equals(left, right);
		}

		/// <summary>
		/// Implements the operator !=.
		/// </summary>
		/// <param name="left">The left.</param>
		/// <param name="right">The right.</param>
		/// <returns>
		/// The result of the operator.
		/// </returns>
		public static bool operator !=(GorgonDepthStencilStates left, GorgonDepthStencilStates right)
		{
			return !GorgonDepthStencilStates.Equals(left, right);
		}
		#endregion

		#region IEquatable<DepthStencilStates> Members
		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>
		/// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
		/// </returns>
		public bool Equals(GorgonDepthStencilStates other)
		{
			return GorgonDepthStencilStates.Equals(ref this, ref other);
		}
		#endregion
	}

	/// <summary>
	/// Depth/stencil buffer state.
	/// </summary>
	/// <remarks>Used to control how depth/stencil testing is applied to a scene.
	/// <para>State objects are immutable.  Therefore, when an application requires a different state, the user must create a new state value, give it the necessary parameters and pass it to the 
	/// appropriate render state.  This is different from previous methods of applying state, where one would modify a render state variable and it would apply immediately.  This model incurred performance penalties 
	/// from too many state changes.  This method does not suffer as much because the states can be reused.  Currently, the system will cache 4096 unique state values for each render state type and 
	/// will reuse these state settings to lower the impact of changing states.
	/// </para>
	/// </remarks>
	public class GorgonDepthStencilRenderState
		: GorgonStateObject<GorgonDepthStencilStates>
	{
		#region Variables.
		private int _depthRef = 0;																// Depth reference.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the depth stencil reference value.
		/// </summary>
		/// <remarks>This is the value used when the stencil state is set to Replace.
		/// <para>The default value is 0.</para>
		/// </remarks>
		public int DepthStencilReference
		{
			get
			{
				return _depthRef;
			}
			set
			{
		#if DEBUG
				if (Graphics.Context == null)
					throw new InvalidOperationException("No usable context was found.");
		#endif
				if (_depthRef != value)
				{
					Graphics.Context.OutputMerger.DepthStencilReference = value;
					_depthRef = value;
				}
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to apply the state to the appropriate state object.
		/// </summary>
		/// <param name="state">The Direct3D state object to apply.</param>
		protected override void ApplyState(IDisposable state)
		{
			Graphics.Context.OutputMerger.DepthStencilState = (D3D.DepthStencilState)state;
		}

		/// <summary>
		/// Function to convert this state object to the native state object type and apply it.
		/// </summary>
		/// <returns>A direct 3D depth/stencil state object.</returns>
		protected override IDisposable Convert()
		{
			D3D.DepthStencilStateDescription desc = new D3D.DepthStencilStateDescription();
			desc.IsDepthEnabled = States.IsDepthEnabled;
			desc.IsStencilEnabled = States.IsStencilEnabled;
			desc.DepthComparison = (D3D.Comparison)States.DepthComparison;
			desc.DepthWriteMask = (States.IsDepthWriteEnabled ? D3D.DepthWriteMask.All : D3D.DepthWriteMask.Zero);
			desc.StencilReadMask = States.StencilReadMask;
			desc.StencilWriteMask = States.StencilWriteMask;

			desc.BackFace = new D3D.DepthStencilOperationDescription();
			desc.FrontFace = new D3D.DepthStencilOperationDescription();

			desc.FrontFace.Comparison = (D3D.Comparison)States.StencilFrontFace.ComparisonOperator;
			desc.FrontFace.DepthFailOperation = (D3D.StencilOperation)States.StencilFrontFace.DepthFailOperation;
			desc.FrontFace.FailOperation = (D3D.StencilOperation)States.StencilFrontFace.FailOperation;
			desc.FrontFace.PassOperation = (D3D.StencilOperation)States.StencilFrontFace.PassOperation;

			desc.BackFace.Comparison = (D3D.Comparison)States.StencilBackFace.ComparisonOperator;
			desc.BackFace.DepthFailOperation = (D3D.StencilOperation)States.StencilBackFace.DepthFailOperation;
			desc.BackFace.FailOperation = (D3D.StencilOperation)States.StencilBackFace.FailOperation;
			desc.BackFace.PassOperation = (D3D.StencilOperation)States.StencilBackFace.PassOperation;

			D3D.DepthStencilState state = new D3D.DepthStencilState(Graphics.D3DDevice, desc);
			state.DebugName = "Gorgon Depth/stencil state #" + StateCacheCount.ToString();

			return state;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonDepthStencilRenderState"/> class.
		/// </summary>
		/// <param name="graphics">The graphics interface that owns this object.</param>
		internal GorgonDepthStencilRenderState(GorgonGraphics graphics)
			: base(graphics, 4096, 15000)
		{
			States = GorgonDepthStencilStates.DefaultStates;
		}
		#endregion
	}
}