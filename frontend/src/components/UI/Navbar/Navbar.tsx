import React, {useEffect, useState} from "react";
import styles from "./Navbar.module.css"
import { getUser } from "../../../utils/utils";
import { useNavigate } from "react-router-dom";
import { GatewayService } from "../../../services/GatewayService";

export const Navbar: React.FC = () => {
	const navigate = useNavigate();
	const user = getUser();
	const isAuthorized = user ? true : false;

	const handleLogout = () => {
		GatewayService.logout();
		navigate('/auth');
	};

	const handleSignIn = () => {
		navigate('/auth');
	};

	const handleRegister = () => {
		navigate('/register');
	};

	return (
		<div className={styles.navbar}>
			<div className={styles.logo_wrapper}>
				МоиБилеты
			</div>
			<ul className={styles.menu_container}>
				<li onClick={() => navigate('/flights')}>
					Рейсы
				</li>
                {user && user.role == "ADMIN" &&
				<li onClick={() => navigate('/stats')}>
					Статистика
				</li>
                }
			</ul>
			<div className={styles.account_info}>
				{isAuthorized?
				<>
					<p onClick={() => navigate('/me')}>
						Моя страница
					</p>
					<button className={styles.navbar_button} onClick={handleLogout}>
						Выйти
					</button>
				</>
				:
				<>
					<button className={styles.navbar_button} onClick={handleSignIn}>
						Войти
					</button>
					<button className={styles.navbar_button} onClick={handleRegister}>
						Зарегистрироваться
					</button>
				</>
				}
			</div>
		</div>
	);
}
