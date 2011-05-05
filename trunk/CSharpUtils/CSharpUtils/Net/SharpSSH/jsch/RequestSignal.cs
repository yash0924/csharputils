using System;

namespace Tamir.SharpSsh.jsch
{
	/* -*-mode:java; c-basic-offset:2; -*- */
	/*
	Copyright (c) 2002,2003,2004 ymnk, JCraft,Inc. All rights reserved.

	Redistribution and use in source and binary forms, with or without
	modification, are permitted provided that the following conditions are met:

	  1. Redistributions of source code must retain the above copyright notice,
		 this list of conditions and the following disclaimer.

	  2. Redistributions in binary form must reproduce the above copyright 
		 notice, this list of conditions and the following disclaimer in 
		 the documentation and/or other materials provided with the distribution.

	  3. The names of the authors may not be used to endorse or promote products
		 derived from this software without specific prior written permission.

	THIS SOFTWARE IS PROVIDED ``AS IS'' AND ANY EXPRESSED OR IMPLIED WARRANTIES,
	INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
	FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL JCRAFT,
	INC. OR ANY CONTRIBUTORS TO THIS SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT,
	INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
	LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA,
	OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
	LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
	NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
	EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
	*/

	class RequestSignal : Request
	{
		String signal="KILL";
		public void setSignal(String foo){ signal=foo; }
		public void request(Session session, Channel channel)
		{
			Buffer buf=new Buffer();
			Packet packet=new Packet(buf);

			packet.reset();
			buf.WriteByte((byte) Session.SSH_MSG_CHANNEL_REQUEST);
			buf.WriteInt(channel.getRecipient());
			buf.WriteString( Util.getBytes("signal"));
			buf.WriteByte((byte)(waitForReply() ? 1 : 0));
			buf.WriteString(Util.getBytes(signal));
			session.write(packet);
		}
		public bool waitForReply(){ return false; }
	}

}
