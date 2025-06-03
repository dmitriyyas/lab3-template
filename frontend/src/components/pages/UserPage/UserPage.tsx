import React, { useEffect, useState } from "react";
import styles from './UserPage.module.css';
import { Navigate, useNavigate, useSearchParams } from "react-router-dom";
import { GatewayService } from "../../../services/GatewayService";
import { deleteJwtToken, getUser, setUser } from "../../../utils/utils";
import { Navbar } from "../../UI/Navbar/Navbar";
import { User, UserInfo } from "../../../types/users";
import { userInfo } from "os";
import { List } from "../../UI/List/List";
import { LeftButton } from "../../UI/LeftButton/LeftButton";
import { RightButton } from "../../UI/RightButton/RightButton";
import { TicketListItem } from "../../UI/TicketListItem/TicketListItem";
import { Modal } from "../../UI/Modal/Modal";
import { Alert } from "../../UI/Alert/Alert";
import { AxiosError } from "axios";
import { Privilege } from "../../../types/privileges";
import { BonusHistoryListItem } from "../../UI/BonusHistoryListItem/BonusHistoryListItem";


export const UserPage: React.FC = () => {
    const navigate = useNavigate();
    const [userInfo, setUserInfo] = useState<UserInfo | null>(null);
    const [privilege, setPrivilege] = useState<Privilege | null>(null);
    const [errorMessage, setErrorMessage] = useState("");
	const [isError, setIsError] = useState(false);
    const [isTicketCanceled, setIsTicketCanceled] = useState(false);

    const isAuthorized = getUser() ? true : false;

    useEffect(() => {
        loadUser();
    }, [])

    const loadUser = async () => {
        GatewayService.getUserInfo()
            .then((res) => {
                setUserInfo(res.data);
            })
            .catch((err) => {
                const axiosErr = err as AxiosError;
                if (axiosErr.status == 401)
                {
                    GatewayService.logout();
                    navigate('/auth')
                }
            });
        GatewayService.getPrivilege()
            .then((res) => {
                setPrivilege(res.data);
            })
            .catch((err) => {
                setPrivilege(null);
            });
    };

    const onCancel = async (ticketUid: string) => {
        try
        {
            await GatewayService.cancelTicket(ticketUid);
            setIsTicketCanceled(true);
            await loadUser();
        }
        catch (err)
        {
            setIsError(true);
            const axiosErr = err as AxiosError;
            if (axiosErr.status == 401)
			{
				GatewayService.logout();
				navigate('/auth')
			}
            setErrorMessage("Ошибка при возврате билета.")
        }
    };

    const ticketsHeader = <TicketListItem isHeader/>;
	const ticketsItems = userInfo?.tickets.map((ticket) => {
		return (
			<TicketListItem key={ticket.ticketUid} ticket={ticket} cancel={() => onCancel(ticket.ticketUid)}/>
		)
	}) ?? [];

    const privilegeHeader = <BonusHistoryListItem isHeader/>;

    let count = 0;
	const privilegeItems = privilege?.history.map((history) => {
        count = history.operationType === "FILL_IN_BALANCE" ? count + history.balanceDiff : count - history.balanceDiff; 
		return (
			<BonusHistoryListItem key={history.date} history={history} count={count}/>
		)
	}) ?? [];

    privilegeItems.reverse();

    if (!isAuthorized)
        return <Navigate to='/auth' />
    
    return (
        <>
        {isError && 
            <Modal>
                <Alert message={errorMessage} onClick={() => setIsError(false)}/>
            </Modal>
        }
        {isTicketCanceled && 
            <Modal>
                <Alert message={'Билет успешно возвращен'} onClick={() => setIsTicketCanceled(false)}/>
            </Modal>
        }
        <Navbar/>
        <div className={styles.body_wrapper}>
            <div className={styles.user_info}>
                <div className={styles.field}>
                    Имя:
                    <p>
                        {userInfo?.user.name}
                    </p>
                </div>
                <div className={styles.field}>
                    Фамилия:
                    <p>
                        {userInfo?.user.surname}
                    </p>
                </div>
                <div className={styles.field}>
                    Отчество:
                    <p>
                        {userInfo?.user.fathername}
                    </p>
                </div>
                <div className={styles.field}>
                    Телефон:
                    <p>
                        {userInfo?.user.phone}
                    </p>
                </div>
                <div className={styles.field}>
                    Эл. Почта:
                    <p>
                        {userInfo?.user.email}
                    </p>
                </div>
                {userInfo?.privilege &&
                <div className={styles.field}>
                    Количество бонусов:
                    <p>
                        {userInfo?.privilege.balance}
                    </p>
                </div>
                }
            </div>
            <List title="Билеты" 
                header={ticketsHeader}
                items={ticketsItems}
                isScrollable
            />
            {privilege &&
            <List title="История бонусов" 
                header={privilegeHeader}
                items={privilegeItems}
                isScrollable
            />
            }
        </div>
        </>
    )
}