using GoogleMobileAds.Ump.Api;
using System;
using System.Collections.Generic;
using Assets.Scripts;
using Assets.Scripts.Libraries.RSG;
using Assets.Scripts.Utils;
using UnityEngine;

namespace GoogleMobileAds.Samples
{
    /// <summary>
    /// Helper class that implements consent using the Google User Messaging Platform (UMP) SDK.
    /// </summary>
    public class GoogleMobileAdsConsentController
    {
        public const string TAG = "[GoogleMobileAdsConsentController] ";
        
        /// <summary>
        /// If true, it is safe to call MobileAds.Initialize() and load Ads.
        /// </summary>
        public bool CanRequestAds => ConsentInformation.CanRequestAds();

        /// <summary>
        /// Startup method for the Google User Messaging Platform (UMP) SDK
        /// which will run all startup logic including loading any required
        /// updates and displaying any required forms.
        /// </summary>
        public IPromise GatherConsent()
        {
            Debug.Log(TAG + "Gathering consent.");
            var result = new Promise();
            
            var requestParameters = new ConsentRequestParameters
            {
                // False means users are not under age.
                TagForUnderAgeOfConsent = false,
                ConsentDebugSettings = new ConsentDebugSettings
                {
                    // For debugging consent settings by geography.
                    DebugGeography = DebugGeography.EEA,
                    // https://developers.google.com/admob/unity/test-ads
                    TestDeviceHashedIds = new List<string>() {"FCC64F954AFBF24727628F8762809311"},
                }
            };

            // The Google Mobile Ads SDK provides the User Messaging Platform (Google's
            // IAB Certified consent management platform) as one solution to capture
            // consent for users in GDPR impacted countries. This is an example and
            // you can choose another consent management platform to capture consent.
            ConsentInformation.Update(requestParameters, (FormError updateError) =>
            {
                // Enable the change privacy settings button.
                UpdatePrivacyButton();

                if (updateError != null)
                {
                    Game.ExecuteOnMainThread(() =>
                    {
                        OnComplete(updateError.Message);
                        result.Resolve();
                    });
                    return;
                }

                // Determine the consent-related action to take based on the ConsentStatus.
                if (CanRequestAds)
                {
                    Game.ExecuteOnMainThread(() =>
                    {
                        Debug.Log(TAG + "Consent has already been gathered or not required");
                        
                        // Consent has already been gathered or not required.
                        // Return control back to the user.
                        OnComplete(null);
                        result.Resolve();
                    });
                    

                    return;
                }

                // Consent not obtained and is required.
                // Load the initial consent request form for the user.
                Game.ExecuteOnMainThread(() =>
                {
                    Debug.Log(TAG + "ConsentForm.LoadAndShowConsentFormIfRequired");
                });
                ConsentForm.LoadAndShowConsentFormIfRequired((FormError showError) =>
                {
                    Game.ExecuteOnMainThread(() =>
                    {
                        Debug.Log(TAG + "ConsentForm.LoadAndShowConsentFormIfRequired callback");
                        UpdatePrivacyButton();
                        OnComplete(showError?.Message);
                        result.Resolve();
                    });
                });
            });

            return result;
        }

        private void OnComplete(string error)
        {
            Debug.Log(TAG + "OnComplete " + error);

            if (error.IsNullOrEmpty())
            {
                Debug.Log(TAG + "IronSource.Agent.setConsent(true)");
                IronSource.Agent.setConsent(true);
            }
            else
            {
                Debug.Log(TAG + "IronSource.Agent.setConsent(false)");
                IronSource.Agent.setConsent(false);
            }
        }

        /// <summary>
        /// Shows the privacy options form to the user.
        /// </summary>
        /// <remarks>
        /// Your app needs to allow the user to change their consent status at any time.
        /// Load another form and store it to allow the user to change their consent status
        /// </remarks>
        public void ShowPrivacyOptionsForm()
        {
            Debug.Log(TAG + "Showing privacy options form.");

            ConsentForm.ShowPrivacyOptionsForm((FormError showError) =>
            {
                Debug.Log(TAG + "Showing privacy options form callback" + showError);
                UpdatePrivacyButton();
                OnComplete(showError?.Message);
            });
        }

        /// <summary>
        /// Reset ConsentInformation for the user.
        /// </summary>
        public void ResetConsentInformation()
        {
            ConsentInformation.Reset();
            UpdatePrivacyButton();
        }

        void UpdatePrivacyButton()
        {
            // if (_privacyButton != null)
            // {
            //     _privacyButton.interactable =
            //         ConsentInformation.PrivacyOptionsRequirementStatus ==
            //             PrivacyOptionsRequirementStatus.Required;
            // }
        }
    }
}