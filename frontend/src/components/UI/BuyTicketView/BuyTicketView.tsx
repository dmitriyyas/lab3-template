import { Flight } from '../../../types/flights';
import { ShortPrivilege } from '../../../types/privileges';
import styles from './BuyTicketView.module.css';

export interface BuyTicketViewProps {
	flight: Flight;
    privilege: ShortPrivilege | null;
	onPayByMoney: () => void;
    onPayByBonuses: () => void;
    onClose: () => void;
}

export const BuyTicketView: React.FC<BuyTicketViewProps> = ({flight, privilege, onPayByMoney, onPayByBonuses, onClose}) => {
    let priceWithBonuses = 0;
    let wastedBonuses = 0;
    if (privilege)
    {
        priceWithBonuses = flight.price - privilege.balance;
        wastedBonuses = privilege.balance;
        if (priceWithBonuses < 0)
        {
            wastedBonuses = privilege.balance - flight.price;
            priceWithBonuses = 0;
        }
    }

	return (
		<div className={styles.body_wrapper}>
			<p>Рейс {flight.flightNumber}</p>
            <p>Цена билета: {flight.price} рублей</p>
            {privilege ?
            <p>На вашем счету {privilege.balance} бонусов</p>
            :
            <p>Информация о бонусах недоступна</p>
            }
            {privilege && privilege.balance > 0 &&
            <button onClick={onPayByBonuses}>
                Купить билет за {priceWithBonuses} рублей и списать {wastedBonuses} бонусов
			</button>
            }
			<button onClick={onPayByMoney}>
                Купить билет за {flight.price} рублей и накопить {flight.price * 0.1} бонусов
			</button>
            <button onClick={onClose}>
                Закрыть
			</button>
	</div>
	)
}
