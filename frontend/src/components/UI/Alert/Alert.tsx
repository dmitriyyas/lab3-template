import styles from './Alert.module.css';

export interface ErrorAlertProps {
	message: string;
	onClick: () => void;
}

export const Alert: React.FC<ErrorAlertProps> = ({message, onClick}) => {
	return (
		<div className={styles.wrapper}>
			<p>{message}</p>
			<button onClick={onClick}>
				ОК
			</button>
	</div>
	)
}
