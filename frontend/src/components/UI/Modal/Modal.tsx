import styles from './Modal.module.css';

export interface ModalProps {
  children: React.ReactNode;
}

export const Modal: React.FC<ModalProps> = ({children}) => {
	return (
		<div className={styles.modal}>
			{children}
		</div>
	)
}
