namespace Core6.Recoverability.Delayed.CustomPolicies
{
    using NServiceBus;
    using NServiceBus.Transport;

    class CustomExceptionPolicy
    {
        CustomExceptionPolicy(EndpointConfiguration endpointConfiguration)
        {
            var recoverability = endpointConfiguration.Recoverability();
            recoverability.Delayed(
                delayed =>
                {
                    delayed.NumberOfRetries(3);
                });
            recoverability.CustomPolicy(MyCustomRetryPolicy);
        }

        #region CustomExceptionPolicyHandler

        RecoverabilityAction MyCustomRetryPolicy(RecoverabilityConfig config, ErrorContext context)
        {
            if (context.Exception is MyBusinessException)
            {
                return RecoverabilityAction.MoveToError(config.Failed.ErrorQueue);
            }

            return DefaultRecoverabilityPolicy.Invoke(config, context);
        }

        #endregion
    }
}