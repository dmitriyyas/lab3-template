import React, { useEffect, useState } from "react";
import { Navbar } from "../../UI/Navbar/Navbar";
import styles from './RegisterPage.module.css';
import { useNavigate, useSearchParams } from "react-router-dom";
import { AxiosError } from "axios";
import { Modal } from "../../UI/Modal/Modal";
import { Alert } from "../../UI/Alert/Alert";
import { GatewayService } from "../../../services/GatewayService";
import { AuthData, UserCreate } from "../../../types/users";
import { deleteJwtToken, setUser } from "../../../utils/utils";


export const RegisterPage: React.FC = () => {
	const navigate = useNavigate();

	const [login, setLogin] = useState("");
	const [password, setPassword] = useState("");
    const [name, setName] = useState("");
    const [surname, setSurname] = useState("");
    const [fathername, setFathername] = useState("");
    const [phone, setPhone] = useState("");
    const [email, setEmail] = useState("");
	const [errorMessage, setErrorMessage] = useState("");
	const [isError, setIsError] = useState(false);
    const [isUserCreated, setIsUserCreated] = useState(false);
    const [searchParams, setSearchParams] = useSearchParams();

	const handleLoginChange = (e: React.ChangeEvent<HTMLInputElement>) => {
		setLogin(e.target.value);
	};

	const handlePasswordChange = (e: React.ChangeEvent<HTMLInputElement>) => {
		setPassword(e.target.value);
	};

    const handleNameChange = (e: React.ChangeEvent<HTMLInputElement>) => {
		setName(e.target.value);
	};

    const handleSurnameChange = (e: React.ChangeEvent<HTMLInputElement>) => {
		setSurname(e.target.value);
	};

    const handleFathernameChange = (e: React.ChangeEvent<HTMLInputElement>) => {
		setFathername(e.target.value);
	};

    const handlePhoneChange = (e: React.ChangeEvent<HTMLInputElement>) => {
		setPhone(e.target.value);
	};

    const handleEmailChange = (e: React.ChangeEvent<HTMLInputElement>) => {
		setEmail(e.target.value);
	};

    const handleRegister = async () => {
        const userCreate: UserCreate = {
            username: login,
            password: password,
            name: name,
            surname: surname,
            fathername: fathername,
            phone: phone,
            email: email
        };

        try
        {
            await GatewayService.register(userCreate);
            setIsUserCreated(true);
        }
        catch (err)
        {
            setIsError(true);
            const axiosErr = err as AxiosError;
            switch (axiosErr.status)
            {
            case 409:
                setErrorMessage('Пользователь с таким логином, телефоном или почтой уже существует.')
                break;
            default:
                setErrorMessage('Ошибка сервера.')
                break;
            }
        }
    };

	return (
	<>
		{isError && 
			<Modal>
				<Alert message={errorMessage} onClick={() => setIsError(false)}/>
			</Modal>
		}
        {isUserCreated && 
            <Modal>
                <Alert message={'Вы успешно зарегистрировались.'} onClick={() => navigate('/auth')}/>
            </Modal>
        }
		<Navbar/>
		<div className={styles.body_wrapper}>
			<div className={styles.auth_container}>
				<div className={styles.input_container}>
                    <div className={styles.field}>
                        Логин
					    <input className={styles.input} value={login} onChange={handleLoginChange}/>
                    </div>
                    <div className={styles.field}>
                        Пароль
					    <input className={styles.input} value={password} onChange={handlePasswordChange}/>
                    </div>
                    <div className={styles.field}>
                        Имя
					    <input className={styles.input} value={name} onChange={handleNameChange}/>
                    </div>
                    <div className={styles.field}>
                        Фамилия
					    <input className={styles.input} value={surname} onChange={handleSurnameChange}/>
                    </div>
                    <div className={styles.field}>
                        Отчество
					    <input className={styles.input} value={fathername} onChange={handleFathernameChange}/>
                    </div>
                    <div className={styles.field}>
                        Телефон
					    <input className={styles.input} value={phone} onChange={handlePhoneChange}/>
                    </div>
                    <div className={styles.field}>
                        Эл. почта
					    <input className={styles.input} value={email} onChange={handleEmailChange}/>
                    </div>
				</div>
				<div className={styles.buttons_container}>
					<button className={styles.button} onClick={handleRegister}>
						Зарегистрироваться
					</button>
				</div>
			</div>
		</div>
	</>
	)
}