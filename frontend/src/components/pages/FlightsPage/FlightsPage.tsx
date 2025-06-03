import { useEffect, useState } from "react";
import styles from './FlightsPage.module.css'
import { AxiosError } from "axios";
import { Flight } from "../../../types/flights";
import { GatewayService } from "../../../services/GatewayService";
import { FlightListItem } from "../../UI/FlightListItem/FlightListItem";
import { Navbar } from "../../UI/Navbar/Navbar";
import { Modal } from "../../UI/Modal/Modal";
import { Alert } from "../../UI/Alert/Alert";
import { List } from "../../UI/List/List";
import { LeftButton } from "../../UI/LeftButton/LeftButton";
import { RightButton } from "../../UI/RightButton/RightButton";
import { getUser, setUser } from "../../../utils/utils";
import { ShortPrivilege } from "../../../types/privileges";
import { BuyTicketView } from "../../UI/BuyTicketView/BuyTicketView";
import { PurchaseRequest } from "../../../types/tickets";
import { useNavigate } from "react-router-dom";

export const FlightsPage: React.FC = () => {
	const navigate = useNavigate();
	const flightsOnPage = 5;
	const [page, setPage] = useState(1);
	const [flights, setFlights] = useState<Flight[]>([]);
	const [errorMessage, setErrorMessage] = useState("");
	const [isError, setIsError] = useState(false);
	const [isTicketPaid, setIsTicketPaid] = useState(false);
	const [flightToBuyTicket, setFlightToBuyTicket] = useState<Flight | null>(null);
	const [privilege, setPrivilege] = useState<ShortPrivilege | null>(null);
	const user = getUser();
	const isAuthorized = user ? true : false;

	const searchFlights = async (page: number) => {
		GatewayService.getFlights(page, flightsOnPage)
			.then((res) => {
				setFlights(res.data.items);
			})
			.catch((err: AxiosError) => {
				setErrorMessage("Ошибка поиска.");
				setIsError(true);
			});
	};

	useEffect(() => {
		searchFlights(1);
	}, []);

	const handlePageRightClick = async () => {
		await searchFlights(page + 1);
		setPage(page + 1);
	};
	
	const handlePageLeftClick = async () => {
		await searchFlights(page - 1);
		setPage(page - 1);
	};

	const handleChooseFlight = async (flightNumber: string) => {
		GatewayService.getPrivilege()
			.then((res) => {
				setPrivilege(res.data);
			})
			.catch((err: AxiosError) => {
				setPrivilege(null);
				if (err.status == 401)
				{
					GatewayService.logout();
					navigate('/auth')
				}
			});
		
		const flight = flights.find(f => f.flightNumber === flightNumber)!
		setFlightToBuyTicket(flight);
	};

	const buyTicket = async (paidFromBalance: boolean) => {
		try
		{
			const purchase: PurchaseRequest = {
				flightNumber: flightToBuyTicket!.flightNumber,
				price: flightToBuyTicket!.price,
				paidFromBalance: paidFromBalance
			};

			await GatewayService.buyTicket(purchase);
			setIsTicketPaid(true);
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
			setErrorMessage("Ошибка при покупке билета.")
		}

		setFlightToBuyTicket(null);
		setPrivilege(null);
	};

	const header = <FlightListItem isHeader/>;
	const items = flights.map((flight) => {
		return (
			<FlightListItem key={flight.flightNumber} flight={flight} chooseFlight={isAuthorized ? handleChooseFlight : undefined}/>
		)
	});

	const navigation = (
		<>
			<div className={styles.buttons_container}>
				<LeftButton isActive={page > 1} onClick={handlePageLeftClick}/>
				<RightButton isActive={flights.length === flightsOnPage} onClick={handlePageRightClick}/>
			</div>
		</>
	)

	return (
		<>
			{isError && 
				<Modal>
					<Alert message={errorMessage} onClick={() => setIsError(false)}/>
				</Modal>
			}
			{isTicketPaid && 
				<Modal>
					<Alert message={'Билет успешно куплен'} onClick={() => setIsTicketPaid(false)}/>
				</Modal>
			}
			{flightToBuyTicket &&
				<Modal>
					<BuyTicketView flight={flightToBuyTicket} 
					privilege={privilege}
					onPayByBonuses={() => buyTicket(true)}
					onPayByMoney={() => buyTicket(false)}
					onClose={() => setFlightToBuyTicket(null)}
				/>
				</Modal>
			}
			<Navbar/>
			<div className={styles.body_wrapper}>
				<List title="Рейсы" 
					header={header}
					items={items}
					navigation={navigation}
					isExpandable
				/>
			</div>
		</>
	)
}
