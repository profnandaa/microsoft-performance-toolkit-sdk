// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Performance.SDK.Processing
{
    public static partial class Projection
    {
        /// <summary>
        ///     Helper class containing useful methods for generating projections.
        /// </summary>
        public static class UlongToDouble
        {
            /// <summary>
            ///     This will create a projection that typecasts a <see cref="ulong"/> value to a <see cref="double"/>.
            /// </summary>
            /// <param name="column">
            ///     Projection that returns an integer.
            /// </param>
            /// <returns>
            ///     Projection that returns a double.
            /// </returns>
            public static IProjection<int, double> Create(IProjection<int, ulong> column)
            {
                Guard.NotNull(column, nameof(column));

                var typeArgs = new[]
                {
                    column.GetType(),
                };

                var constructorArgs = new object[]
                {
                    column,
                };

                var type = typeof(UlongToDoubleColumnGenerator<>).MakeGenericType(typeArgs);
                var instance = Activator.CreateInstance(type, constructorArgs);
                return (IProjection<int, double>)instance;
            }

            private struct UlongToDoubleColumnGenerator<TGenerator>
                : IProjection<int, double>,
                  IViewportSensitiveProjection
                  where TGenerator : IProjection<int, ulong>
            {
                private readonly TGenerator generator;

                public UlongToDoubleColumnGenerator(TGenerator generator)
                {
                    this.generator = generator;
                }

                public double this[int value]
                {
                    get
                    {
                        return (double)this.generator[value];
                    }
                }

                public Type SourceType
                {
                    get { return typeof(int); }
                }

                public Type ResultType
                {
                    get { return typeof(double); }
                }

                // IViewportSensitiveProjection
                public object Clone()
                {
                    if (DependsOnViewport)
                    {
                        return new UlongToDoubleColumnGenerator<TGenerator>(
                            ViewportSensitiveProjection.CloneIfViewportSensitive(this.generator));
                    }
                    else
                    {
                        return this;
                    }
                }

                public bool NotifyViewportChanged(IVisibleTableRegion viewport)
                {
                    return ViewportSensitiveProjection.NotifyViewportChanged(this.generator, viewport);
                }

                public bool DependsOnViewport
                {
                    get
                    {
                        return ViewportSensitiveProjection.DependsOnViewport(this.generator);
                    }
                }
            }
        }
    }
}
