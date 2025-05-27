import { useEffect, useState } from "react";
import styles from './StatsPage.module.css'
import { AxiosError } from "axios";
import { GatewayService } from "../../../services/GatewayService";
import { Navbar } from "../../UI/Navbar/Navbar";
import { Modal } from "../../UI/Modal/Modal";
import { Alert } from "../../UI/Alert/Alert";
import { List } from "../../UI/List/List";
import { LeftButton } from "../../UI/LeftButton/LeftButton";
import { RightButton } from "../../UI/RightButton/RightButton";
import { getUser, setUser } from "../../../utils/utils";
import { Stat } from "../../../types/stats";
import { StatListItem } from "../../UI/StatListItem/StatListItem";
import { Navigate, useNavigate } from "react-router-dom";

export const StatsPage: React.FC = () => {
    const navigate = useNavigate();
	const statsOnPage = 5;
	const [page, setPage] = useState(1);
	const [stats, setStats] = useState<Stat[]>([]);
	const [errorMessage, setErrorMessage] = useState("");
	const [isError, setIsError] = useState(false);
	const user = getUser();
	const isAuthorized = user ? true : false;

	const searchStats = async (page: number) => {
		GatewayService.getStats(page, statsOnPage)
			.then((res) => {
				setStats(res.data.items);
			})
			.catch((err: AxiosError) => {
				setErrorMessage("Ошибка поиска.");
				setIsError(true);

                if (err.code == '401')
                {
                    GatewayService.logout();
                    navigate('/auth')
                }
			});
	};

	useEffect(() => {
		searchStats(1);
	}, []);

	const handlePageRightClick = async () => {
		await searchStats(page + 1);
		setPage(page + 1);
	};
	
	const handlePageLeftClick = async () => {
		await searchStats(page - 1);
		setPage(page - 1);
	};

	const header = <StatListItem isHeader/>;
	const items = stats.map((stat) => {
		return (
			<StatListItem key={stat.id} stat={stat} />
		)
	});

	const navigation = (
		<>
			<div className={styles.buttons_container}>
				<LeftButton isActive={page > 1} onClick={handlePageLeftClick}/>
				<RightButton isActive={stats.length === statsOnPage} onClick={handlePageRightClick}/>
			</div>
		</>
	)

    if (!(user && user.role == "ADMIN"))
        return <Navigate to='/' />

	return (
		<>
			{isError && 
				<Modal>
					<Alert message={errorMessage} onClick={() => setIsError(false)}/>
				</Modal>
			}
			<Navbar/>
			<div className={styles.body_wrapper}>
				<List title="Запросы" 
					header={header}
					items={items}
					navigation={navigation}
					isExpandable
				/>
			</div>
		</>
	)
}
