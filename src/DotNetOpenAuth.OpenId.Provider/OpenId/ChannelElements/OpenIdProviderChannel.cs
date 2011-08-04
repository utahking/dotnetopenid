﻿//-----------------------------------------------------------------------
// <copyright file="OpenIdProviderChannel.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.OpenId.Provider;
	using DotNetOpenAuth.Messaging.Bindings;
	using System.Diagnostics.Contracts;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.Extensions;

	internal class OpenIdProviderChannel : OpenIdChannel {
		/// <summary>
		/// Initializes a new instance of the <see cref="OpenIdProviderChannel"/> class.
		/// </summary>
		/// <param name="cryptoKeyStore">The OpenID Provider's association store or handle encoder.</param>
		/// <param name="nonceStore">The nonce store to use.</param>
		/// <param name="securitySettings">The security settings.</param>
		internal OpenIdProviderChannel(IProviderAssociationStore cryptoKeyStore, INonceStore nonceStore, ProviderSecuritySettings securitySettings)
			: this(cryptoKeyStore, nonceStore, new OpenIdMessageFactory(), securitySettings) {
			Contract.Requires<ArgumentNullException>(cryptoKeyStore != null);
			Contract.Requires<ArgumentNullException>(securitySettings != null);
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="OpenIdProviderChannel"/> class.
		/// </summary>
		/// <param name="cryptoKeyStore">The association store to use.</param>
		/// <param name="nonceStore">The nonce store to use.</param>
		/// <param name="messageTypeProvider">An object that knows how to distinguish the various OpenID message types for deserialization purposes.</param>
		/// <param name="securitySettings">The security settings.</param>
		private OpenIdProviderChannel(IProviderAssociationStore cryptoKeyStore, INonceStore nonceStore, IMessageFactory messageTypeProvider, ProviderSecuritySettings securitySettings)
			: base(messageTypeProvider, InitializeBindingElements(cryptoKeyStore, nonceStore, securitySettings)) {
				Contract.Requires<ArgumentNullException>(cryptoKeyStore != null);
			Contract.Requires<ArgumentNullException>(messageTypeProvider != null);
			Contract.Requires<ArgumentNullException>(securitySettings != null);
		}

		/// <summary>
		/// Initializes the binding elements.
		/// </summary>
		/// <param name="cryptoKeyStore">The OpenID Provider's crypto key store.</param>
		/// <param name="nonceStore">The nonce store to use.</param>
		/// <param name="securitySettings">The security settings to apply.  Must be an instance of either <see cref="RelyingPartySecuritySettings"/> or <see cref="ProviderSecuritySettings"/>.</param>
		/// <returns>
		/// An array of binding elements which may be used to construct the channel.
		/// </returns>
		private static IChannelBindingElement[] InitializeBindingElements(IProviderAssociationStore cryptoKeyStore, INonceStore nonceStore, ProviderSecuritySettings securitySettings) {
			Contract.Requires<ArgumentNullException>(cryptoKeyStore != null);
			Contract.Requires<ArgumentNullException>(securitySettings != null);
			Contract.Requires<ArgumentNullException>(nonceStore != null);

			SigningBindingElement signingElement;
			signingElement = new ProviderSigningBindingElement(cryptoKeyStore, securitySettings);

			var extensionFactory = OpenIdExtensionFactoryAggregator.LoadFromConfiguration();

			List<IChannelBindingElement> elements = new List<IChannelBindingElement>(8);
			elements.Add(new ExtensionsBindingElement(extensionFactory, securitySettings, true));
			elements.Add(new StandardReplayProtectionBindingElement(nonceStore, true));
			elements.Add(new StandardExpirationBindingElement());
			elements.Add(signingElement);

			return elements.ToArray();
		}
	}
}
