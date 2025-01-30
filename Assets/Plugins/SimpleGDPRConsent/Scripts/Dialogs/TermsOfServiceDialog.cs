using SimpleGDPRConsent;
using System;

public class TermsOfServiceDialog : IGDPRDialog
{
	private string termsOfServiceLink;
	private string privacyPolicyLink;
	public static Action TermsOfServiceOpened;
	public static Action PrivacyPolicyOpened;

	public TermsOfServiceDialog()
	{ 
	}

	public TermsOfServiceDialog SetTermsOfServiceLink(string termsOfServiceLink, Action onClick)
	{
		this.termsOfServiceLink = termsOfServiceLink;
		TermsOfServiceOpened = onClick;
		return this;
	}

	public TermsOfServiceDialog SetPrivacyPolicyLink(string privacyPolicyLink, Action onClick)
	{
		this.privacyPolicyLink = privacyPolicyLink;
		PrivacyPolicyOpened = onClick;
		return this;
	}

	void IGDPRDialog.ShowDialog(SimpleGDPR.DialogClosedDelegate onDialogClosed)
	{
		GDPRConsentCanvas.Instance.ShowTermsOfServiceDialog(termsOfServiceLink, privacyPolicyLink, onDialogClosed);
	}
}