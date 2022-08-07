import React from 'react';
import { toast } from 'react-toastify';
import { Button, Header, Icon, Segment } from 'semantic-ui-react';
import agent from '../../app/api/agent';
import useQuery from '../../common/util/hooks';

export default function RegisterSuccess() {
    const email = useQuery().get('email') as string;

    function handleConfirmEmailResend() {
        agent.Account.resendEmailConfirm(email).then(() => {
            toast.success('Verification email resent - please check your email');
        }).catch(error => console.log('error resending email confirmation ', error));
    }

    return (
        <Segment placeholder textAlign='center'>
            <Header icon color='green'>
                <Icon name='check' />
                <p>Please check your email(including junk email) for the veirfication email</p>
                {email &&
                    <>
                        <p>
                            Did not receive the email? Click the below button to resend
                        </p>
                        <Button primary onClick={handleConfirmEmailResend} content='Resend Email Confirmation' size='huge'/>
                    </>
                }
            </Header>
        </Segment>
    )
}