// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace DotNetty.Codecs
{
    using System;

    public class EncoderException : Exception
    {
        public EncoderException(string message)
            : base(message)
        {
        }

        public EncoderException(Exception innerException)
            : base(null, innerException)
        {
        }
    }
}