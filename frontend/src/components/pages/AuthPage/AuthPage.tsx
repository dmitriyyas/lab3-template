import React, { useEffect, useState } from "react";
import { Navbar } from "../../UI/Navbar/Navbar";
import styles from './AuthPage.module.css';
import { useNavigate, useSearchParams } from "react-router-dom";
import { AxiosError } from "axios";
import { Modal } from "../../UI/Modal/Modal";
import { Alert } from "../../UI/Alert/Alert";
import { GatewayService } from "../../../services/GatewayService";
import { AuthData } from "../../../types/users";
import { deleteJwtToken, setUser } from "../../../utils/utils";


export const AuthPage: React.FC = () => {
	const navigate = useNavigate();

	const [login, setLogin] = useState("Логин");
	const [password, setPassword] = useState("Пароль");
	const [errorMessage, setErrorMessage] = useState("");
	const [isError, setIsError] = useState(false);
    const [searchParams, setSearchParams] = useSearchParams();

	useEffect(() => {
		const code = searchParams.get("code");
		const state = searchParams.get("state");
		if (code && state)
		{
			if (state === sessionStorage.getItem('state'))
			{
				sessionStorage.removeItem('state');
				GatewayService.getToken(code)
				.then((token) => {
					GatewayService.getUserInfo()
						.then((res) => {
							setUser(res.data.user);
							navigate('/');
						})
						.catch((err) => { 
							deleteJwtToken();
							setIsError(true);
							setErrorMessage("Ошибка входа.") 
						})
				})
				.catch((err) => {
					setIsError(true);
					setErrorMessage("Ошибка входа.") 
				})
			}
		}
	}, []);

	const handleLoginChange = (e: React.ChangeEvent<HTMLInputElement>) => {
		setLogin(e.target.value);
	};

	const handlePasswordChange = (e: React.ChangeEvent<HTMLInputElement>) => {
		setPassword(e.target.value);
	};

	const handleSignIn = async () => {
        const redirectUri = searchParams.get("redirect_uri") ?? 'http://localhost/auth';
        const clientId = searchParams.get("client_id") ?? "Audience";
        const scope = searchParams.get("scope") ?? "openid profile email";
        let state = searchParams.get("state");

        if (!state)
        {
            state = Math.random().toString();
            sessionStorage.setItem('state', state);
        }

        const authData: AuthData = {
            username: login,
            password: password,
            redirectUri: redirectUri,
            clientId: clientId,
            state: state,
            scope: scope
        }

        try
        {
            const response = await GatewayService.login(authData);
            window.location.href = response.data;
        }
        catch (err)
        {
            setIsError(true);
            setErrorMessage("Ошибка при входе.")
            console.log(err);
        }
	};

	return (
	<>
		{isError && 
			<Modal>
				<Alert message={errorMessage} onClick={() => setIsError(false)}/>
			</Modal>
		}
		<Navbar/>
		<div className={styles.body_wrapper}>
			<div className={styles.auth_container}>
				<div className={styles.input_container}>
					<input className={styles.input} value={login} onChange={handleLoginChange}/>
					<input className={styles.input} value={password} onChange={handlePasswordChange}/>
				</div>
				<div className={styles.buttons_container}>
					<button className={styles.button} onClick={handleSignIn}>
						Войти
					</button>
				</div>
			</div>
		</div>
	</>
	)
}
